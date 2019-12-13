using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;
using System.Data.Entity;
using BankService.DatabaseManagement.Repositories;
using Common;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager, IDisposable
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private readonly DbContext dbContext;
		private static readonly string connectionString;

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
			dbContext = ServiceLocator.GetObject<DbContext>();
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

			EnqueueNotSentCommands();
		}

		private void EnqueueNotSentCommands()
		{
			IEnumerable<BaseCommand> commands = databaseManager.Find(x => x.State == CommandState.NotSent);

			foreach (BaseCommand command in commands)
			{
				SendCommandToSpecificQueue(command);
			}
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
			dbContext.Database.CreateIfNotExists();
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

		private void SendCommandToSpecificQueue(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);

		}

		public long EnqueueCommand(BaseCommand command)
		{
			databaseManager.AddEntity(command);
			SendCommandToSpecificQueue(command);

			return command.ID;

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
			try
			{
				dbContext.Database.Connection.Open();
			}
			catch { }

			IRepository<BaseCommand> commandRepository = ServiceLocator.GetService<IRepository<BaseCommand>>();
			databaseManager = new DatabaseManager<BaseCommand>(commandRepository);
		}
	}
}
