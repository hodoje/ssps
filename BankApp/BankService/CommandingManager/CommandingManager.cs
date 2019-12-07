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

		private CancellationTokenSource cancellationToken;
		private List<ICommandingHost> commandingHosts;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private ConcurrentQueue<CommandNotification> responseQueue;

		private DatabaseManager databaseManager;

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

			commandToQueueMapper = new Dictionary<Type, CommandQueue>(supportedCommands.Count);

			InitializeDatabase();

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

		public void EnqueueCommand(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);
			databaseManager.SaveCommand(command);

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
					Thread.Sleep(300);
					continue;
				}

				// todo send notification to unit responsible for user notification
			}
		}

		private void InitializeDatabase()
		{
			IDataPersistence databasePersistence = new XmlDataPersistence();
			databaseManager = new DatabaseManager(databasePersistence);
		}
	}
}
