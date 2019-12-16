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
using System.Linq;
using System.ServiceModel;
using System.Net.Security;

namespace BankService.CommandingManager
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class CommandingManager : ICommandingManager, IDisposable, IStartupConfirmationService, IBankAliveService
	{
		private class CommandsPerQueue
		{
			public CommandsPerQueue(string sectorName, List<Type> commands)
			{
				SectorName = sectorName;
				Commands = commands;
			}

			public string SectorName { get; private set; }
			public List<Type> Commands { get; private set; }
		}

		private static readonly int queueSize;
		private static readonly int timeoutPeriod;
		private readonly DbContext dbContext;
		private static readonly string connectionString;

		private Dictionary<Type, CommandQueue> commandToQueueMapper;
		private ConcurrentQueue<CommandNotification> responseQueue;
		private List<ICommandingHost> commandingHosts;
		private List<CommandsPerQueue> supportedCommands;

		private IAudit auditService;

		private IDatabaseManager<BaseCommand> databaseManager;

		private ServiceHost startupConfirmationHost;
		private ServiceHost bankAliveServiceHost;

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

			supportedCommands = new List<CommandsPerQueue>(3)
			{
				// AllSectorNames mapped from config file (json)
				new CommandsPerQueue(BankServiceConfig.AllSectorNames[2], new List<Type>(2){ typeof(DepositCommand), typeof(WithdrawCommand) }),
				new CommandsPerQueue(BankServiceConfig.AllSectorNames[1], new List<Type>() { typeof(RequestLoanCommand) }),
				new CommandsPerQueue(BankServiceConfig.AllSectorNames[0], new List<Type>() { typeof(RegistrationCommand) })
			};

			InitialDatabaseLoading();

			commandToQueueMapper = new Dictionary<Type, CommandQueue>(supportedCommands.Count);

			commandingHosts = new List<ICommandingHost>(supportedCommands.Count);
			//CreateCommandingHosts(commandToQueueMapper, supportedCommands);

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

		//private void CreateCommandingHosts(Dictionary<Type, CommandQueue> commandToQueueMapper, List<CommandsPerQueue> supportedCommands)
		//{
		//	foreach (CommandsPerQueue commandsPerQueue in supportedCommands)
		//	{
		//		string commandingHostString = String.Join(", ", commandsPerQueue.Commands.Select(x => x.Name));
		//		CommandQueue newQueue = new CommandQueue(queueSize, timeoutPeriod);
		//		CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(auditService, newQueue, responseQueue, BankServiceConfig.Connections[commandsPerQueue.SectorName], databaseManager, commandingHostString);
		//		commandingHosts.Add(newHost);

		//		foreach (Type commandType in commandsPerQueue.Commands)
		//		{
		//			commandToQueueMapper.Add(commandType, newQueue);
		//		}

		//		newHost.Start();

		//		auditService.Log($"CommandingHost[{commandingHostString}]", "Initialized.");
		//	}
		//}

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

		public void StartListeningForAlivePings()
		{
			bankAliveServiceHost = new ServiceHost(this);
			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.Sign;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			var address = $"{BankServiceConfig.BankAliveServiceAddress}/{BankServiceConfig.BankAliveServiceEndpointName}";
			bankAliveServiceHost.AddServiceEndpoint(typeof(IBankAliveService), binding, address);
			try
			{
				bankAliveServiceHost.Open();
			}
			catch(Exception e)
			{
				throw;
			}
		}

		public void StartListeningForConnectedSectors()
		{
			startupConfirmationHost = new ServiceHost(this);
			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.Sign;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			string address = $"{BankServiceConfig.StartupConfirmationServiceAddress}/{BankServiceConfig.StartupConfirmationServiceEndpointName}";
			startupConfirmationHost.AddServiceEndpoint(typeof(IStartupConfirmationService), binding, address);
			try
			{
				startupConfirmationHost.Open();
			}
			catch(Exception e)
			{
				throw;
			}
		}

		public void OpenCommandHostForSector(string sectorType)
		{
			CommandsPerQueue commandsPerQueue = supportedCommands.FirstOrDefault(x => x.SectorName == sectorType);
			string commandingHostString = String.Join(", ", commandsPerQueue.Commands.Select(x => x.Name));
			CommandQueue newQueue = new CommandQueue(queueSize, timeoutPeriod);
			CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(sectorType, auditService, newQueue, responseQueue, BankServiceConfig.Connections[commandsPerQueue.SectorName], databaseManager, commandingHostString);
			commandingHosts.Add(newHost);

			foreach (Type commandType in commandsPerQueue.Commands)
			{
				commandToQueueMapper.Add(commandType, newQueue);
			}

			newHost.Start();

			auditService.Log($"CommandingHost[{commandingHostString}]", "Initialized.");
		}

		public void ConfirmStartup(string sectorType)
		{
			Console.WriteLine($"{sectorType.ToUpper()} connected.");
			OpenCommandHostForSector(sectorType);
		}

		public void CheckIfBankAlive(string callingSector)
		{
			Console.WriteLine($"{callingSector.ToUpper()} checking if I'm alive.");
		}
	}
}
