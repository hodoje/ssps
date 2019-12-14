using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;
using System.Data.Entity;
using BankService.DatabaseManagement.Repositories;
using Common;
using Common.Communication;
using Common.ServiceInterfaces;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager, IDisposable
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private readonly DbContext dbContext;
		private static readonly string connectionString;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private ConcurrentQueue<CommandNotification> responseQueue;
		private List<ICommandingHost> commandingHosts;

		private IAudit auditService;

		private IDatabaseManager<BaseCommand> databaseManager;

		static CommandingManager()
		{
			// TODO read from configuration
			queueSize = 5;
			timeoutPeriod = 60;
		}

		public CommandingManager(IAudit auditService, ConcurrentQueue<CommandNotification> responseQueue)
		{
			dbContext = ServiceLocator.GetObject<DbContext>();
			this.auditService = auditService;
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
				auditService.Log(logMessage: $"Command({command.GetType().Name} - id = {command.ID}) enqueued to commanding queue from database");
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

				foreach (BaseCommand expiredCommand in expiredCommands)
				{
					commandingQueue.RemoveCommandById(expiredCommand.ID);
					databaseManager.RemoveEntity(expiredCommand.ID);
					auditService.Log(logMessage: $"Command(id = {expiredCommand.ID}) was in timeout period and is removed from commanding queue and database.");
				}
			}
		}

		public void CreateDatabase()
		{
			dbContext.Database.CreateIfNotExists();
			auditService.Log(logMessage: "New database created!", eventLogEntryType: System.Diagnostics.EventLogEntryType.Warning);
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

			auditService.Log("CommandManager", $"{command.GetType().Name}(id = {command.ID}) added to database!");

			SendCommandToSpecificQueue(command);

			return command.ID;

		}

		private void CreateCommandingHosts(Dictionary<Type, CommandQueue> commandToQueueMapper, List<Type> commandTypes)
		{
			foreach (Type commandType in commandTypes)
			{
				CommandQueue newQueue = new CommandQueue(queueSize, timeoutPeriod);
				CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(auditService, newQueue, responseQueue, BankServiceConfig.Connections[commandType], databaseManager, commandType.Name);
				commandingHosts.Add(newHost);

				commandToQueueMapper.Add(commandType, newQueue);
				newHost.Start();

				auditService.Log($"CommandingHost[{commandType.Name}]", "Initialized.");
			}
		}

		private void InitialDatabaseLoading()
		{
			try
			{
				dbContext.Database.Connection.Open();
				auditService.Log(logMessage: "Database connection opened.", eventLogEntryType: System.Diagnostics.EventLogEntryType.Information);
			}
			catch { }

			IRepository<BaseCommand> commandRepository = ServiceLocator.GetService<IRepository<BaseCommand>>();
			databaseManager = new DatabaseManager<BaseCommand>(commandRepository);
		}
	}
}
