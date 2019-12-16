using Common.Commanding;
using Common.ServiceInterfaces;
using System.ServiceModel;
using BankService.CommandingManager;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BankService.Notification;
using Common;
using BankService.DatabaseManagement.Repositories;
using BankService.DatabaseManagement;
using System.Data.Entity;
using System.Threading;
using Common.CertificateManagement;
using Common.Communication;
using System.Security.Permissions;
using Common.Model;
using System.Security;

namespace BankService
{
	/// <summary>
	/// Class represents banking service.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class BankingService : IUserService, IAdminService
	{
		private readonly string connectionString;
		private object locker = new object();

		private ICommandingManager commandManager;
		private INotificationHandler notificationHandler;
		private IAudit auditService; 

		private ConcurrentQueue<CommandNotification> responseQueue;
		public BankingService()
		{
			connectionString = "BankServiceDB";

			InitializesObjects();

			auditService = new AuditClientProxy(BankServiceConfig.AuditServiceAddress, BankServiceConfig.AuditServiceEndpointName);

			responseQueue = new ConcurrentQueue<CommandNotification>();
			notificationHandler = new NotificationHandler(auditService, responseQueue, new NotificationContainer(ServiceLocator.GetService<IRepository<CommandNotification>>()));

			commandManager = new CommandingManager.CommandingManager(auditService, responseQueue);

			//FOR TESTING
			//commandManager.CreateDatabase();
		}

		public void StartListeningForSectorConnections()
		{
			commandManager.StartListeningForConnectedSectors();
		}

		public void StartListeningForCheckAlivePings()
		{
			commandManager.StartListeningForAlivePings();
		}

		public void CreateNewDatabase()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("admins"))
			{
				auditService.Log(username, "Failed authorization on creating new database command", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, "Authorized as admin, create data base access granted.", System.Diagnostics.EventLogEntryType.Warning);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			commandManager.CreateDatabase();
		}

		public void DeleteStaleCommands()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("admins"))
			{
				auditService.Log(username, "Failed authorization on deleting commands which are in timeout period.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}
			auditService.Log(username, "Authorized as admin, delete timeout command initiated.", System.Diagnostics.EventLogEntryType.Warning);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			commandManager.ClearStaleCommands();
		}

		public void Deposit(double amount)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("users"))
			{
				auditService.Log(username, "Failed authorization on requesting deposit.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, $"Authorized as user, {amount} requested.", System.Diagnostics.EventLogEntryType.Information);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			TransactionCommand depositCommand = new TransactionCommand(0, username, amount, TransactionType.Deposit);
			long commandId = commandManager.EnqueueCommand(depositCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		public List<CommandNotification> GetPendingNotifications()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("users"))
			{
				auditService.Log(username, "Failed authorization on requesting pending notifications.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, $"Authorized as user, asks for pending notifications.", System.Diagnostics.EventLogEntryType.Information);
			List<CommandNotification> userNotifications = notificationHandler.GetUserNotifications(username);

			return userNotifications;
		}

		public void Register()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("users"))
			{
				auditService.Log(username, "Failed authorization on register.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, $"Authorized as user, asks for registration.", System.Diagnostics.EventLogEntryType.Information);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			RegistrationCommand registrationCommand = new RegistrationCommand(0, username);
			long commandId = commandManager.EnqueueCommand(registrationCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		public void RequestLoan(double amount, int months)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("users"))
			{
				auditService.Log(username, "Failed authorization on requesting loan.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, $"Authorized as user, requests loan of {amount}.", System.Diagnostics.EventLogEntryType.Information);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(0, username, amount, months);
			long commandId = commandManager.EnqueueCommand(requestLoanCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		public void Withdraw(double amount)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			if (!Thread.CurrentPrincipal.IsInRole("users"))
			{
				auditService.Log(username, "Failed authorization on requesting withdraw.", System.Diagnostics.EventLogEntryType.Warning);
				throw new SecurityException("Access is denied.");
			}

			auditService.Log(username, $"Authorized as user, requests loan of {amount}.", System.Diagnostics.EventLogEntryType.Information);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			TransactionCommand withdrawCommand = new TransactionCommand(0, username, amount, TransactionType.Withdraw);
			long commandId = commandManager.EnqueueCommand(withdrawCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		private void InitializesObjects()
		{
			BankContext bankContext = new BankContext(connectionString);
			SemaphoreSlim semaphore = new SemaphoreSlim(1);
			ServiceLocator.RegisterObject<DbContext>(bankContext);
			ServiceLocator.RegisterService<IRepository<BaseCommand>>(new Repository<BaseCommand>(bankContext, semaphore));
			ServiceLocator.RegisterService<IRepository<CommandNotification>>(new Repository<CommandNotification>(bankContext, semaphore));
		}
	}
}
