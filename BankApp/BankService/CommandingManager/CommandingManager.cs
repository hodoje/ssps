using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;
using Common.ServiceInterfaces;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager, IDisposable
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private static readonly string xmlFilePath;

		private List<ICommandingHost> commandingHosts;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private IDatabaseManager databaseManager;
		private INotificationHandler notificationHandler;
		private ConcurrentQueue<CommandNotification> responseQueue;

		static CommandingManager()
		{
			// TODO read from configuration
			queueSize = 5;
			timeoutPeriod = 3;
		}

		public CommandingManager()
		{
			responseQueue = new ConcurrentQueue<CommandNotification>();

			List<Type> supportedCommands = new List<Type>(4)
			{
				typeof(DepositCommand),
				typeof(WithdrawCommand),
				typeof(RequestLoanCommand),
				typeof(RegistrationCommand)
			};

			InitialDatabaseLoading();

			commandToQueueMapper = new Dictionary<Type, CommandQueue>(supportedCommands.Count);

			commandingHosts = new List<ICommandingHost>(supportedCommands.Count);
			CreateCommandingHosts(commandToQueueMapper, supportedCommands);

			InitializeNotificationHandler();
		}

		public bool CancelCommand(long commandId)
		{
			foreach (CommandQueue commandingQueue in commandToQueueMapper.Values)
			{
				if (commandingQueue.RemoveCommandById(commandId))
				{
					return true;
				}
			}

			return false;
		}

		public void ClearStaleCommands()
		{
			foreach (CommandQueue commandingQueue in commandToQueueMapper.Values)
			{
				List<BaseCommand> expiredCommands = commandingQueue.ExpiredCommands();

				expiredCommands.ForEach(x => commandingQueue.RemoveCommandById(x.CommandId));

				expiredCommands.ForEach(x => databaseManager.RemoveCommand(x.CommandId));
			}
		}

		public void CreateDatabase()
		{
			IDataPersistence databasePersistence = new XmlDataPersistence(xmlFilePath);
			databaseManager.LoadNewDataPersitenceUnit(databasePersistence);
		}

		public void Dispose()
		{
			((IDisposable)databaseManager).Dispose();
			((IDisposable)notificationHandler).Dispose();

			foreach (ICommandingHost commandingHost in commandingHosts)
			{
				((IDisposable)commandingHost).Dispose();
			}

			commandToQueueMapper.Clear();
			
		}

		public void EnqueueCommand(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);
			databaseManager.SaveCommand(command);

			// todo log
		}

		public void RegisterClient(string key, IUserServiceCallback userCallback)
		{
			notificationHandler.TryRegisterUserForNotifications(key, userCallback);
		}

		private void CreateCommandingHosts(Dictionary<Type, CommandQueue> commandToQueueMapper, List<Type> commandTypes)
		{
			foreach (Type commandType in commandTypes)
			{
				CommandQueue newQueue = new CommandQueue(queueSize, timeoutPeriod);
				CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(newQueue, responseQueue, BankServiceConfig.Connections[commandType], databaseManager, commandType.Name);
				commandingHosts.Add(newHost);

				commandToQueueMapper.Add(commandType, newQueue);
				newHost.Start();
			}
		}

		private void InitialDatabaseLoading()
		{
			IDataPersistence databasePersistence = XmlDataPersistence.CreateParser(xmlFilePath);
			databaseManager = new DatabaseManager(databasePersistence);
		}

		private void InitializeNotificationHandler()
		{
			notificationHandler = new NotificationHandler(responseQueue);
			notificationHandler.Start();
		}
	}
}
