using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager, IDisposable
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private static readonly string xmlFilePath;

		private List<ICommandingHost> commandingHosts;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private IDatabaseManager<BaseCommand> databaseManager;
		private ConcurrentQueue<CommandNotification> responseQueue;

		static CommandingManager()
		{
			// TODO read from configuration
			queueSize = 5;
			timeoutPeriod = 3;
		}

		public CommandingManager(ConcurrentQueue<CommandNotification> responseQueue)
		{
			this.responseQueue = responseQueue;

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

				expiredCommands.ForEach(x => commandingQueue.RemoveCommandById(x.ID));

				expiredCommands.ForEach(x => databaseManager.RemoveEntity(x.ID));
			}
		}

		public void CreateDatabase()
		{
			IDataPersistence<BaseCommand> databasePersistence = (IDataPersistence<BaseCommand>)new XmlDataPersistence<BaseCommand>(xmlFilePath);
			databaseManager.LoadNewDataPersitenceUnit(databasePersistence);
		}

		public void Dispose()
		{
			((IDisposable)databaseManager).Dispose();

			foreach (ICommandingHost commandingHost in commandingHosts)
			{
				((IDisposable)commandingHost).Dispose();
			}

			commandToQueueMapper.Clear();
			
		}

		public void EnqueueCommand(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);
			databaseManager.SaveEntity(command);

			// todo log
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
			IDataPersistence<BaseCommand> databasePersistence = XmlDataPersistence<BaseCommand>.CreatePersister(xmlFilePath);
			databaseManager = new DatabaseManager<BaseCommand>(databasePersistence);
		}
	}
}
