using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using System.Threading;
using BankService.DatabaseManagement;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private static readonly string xmlFilePath;

		private CancellationTokenSource cancellationToken;
		private List<ICommandingHost> commandingHosts;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private DatabaseManager databaseManager;
		private ConcurrentQueue<CommandNotification> responseQueue;

		static CommandingManager()
		{
			// TODO read from configuration
			queueSize = 5;
			timeoutPeriod = 3;
		}

		public CommandingManager()
		{
			cancellationToken = new CancellationTokenSource();
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

			Task listenWorker = new Task(ListenForCommandNotifications);
			listenWorker.Start();
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

		public void EnqueueCommand(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);
			databaseManager?.SaveCommand(command);

			// todo log
		}

		private void CreateCommandingHosts(Dictionary<Type, CommandQueue> commandToQueueMapper, List<Type> commandTypes)
		{
			foreach (Type commandType in commandTypes)
			{
				CommandQueue newQueue = new CommandQueue(queueSize, timeoutPeriod);
				CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(newQueue, responseQueue, Configuration.Instance.Connections[commandType], databaseManager, commandType.Name);
				commandingHosts.Add(newHost);

				commandToQueueMapper.Add(commandType, newQueue);
				newHost.Start();
			}
		}

		private void ListenForCommandNotifications()
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				CommandNotification notification;
				if (!responseQueue.TryDequeue(out notification))
				{
					Thread.Sleep(2000);
					continue;
				}

				// todo send notification to unit responsible for user notification
			}
		}

		private void InitialDatabaseLoading()
		{
			IDataPersistence databasePersistence = XmlDataPersistence.CreateParser(xmlFilePath);
			databaseManager = new DatabaseManager(databasePersistence);
		}
	}
}
