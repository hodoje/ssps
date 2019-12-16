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
			connectionString = "TEST";

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

		[PrincipalPermission(SecurityAction.Demand, Role = "admins")]
		public void CreateNewDatabase()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			commandManager.CreateDatabase();
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "admins")]
		public void DeleteStaleCommands()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			commandManager.ClearStaleCommands();
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "users")]
		public void Deposit(double amount)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			DepositCommand depositCommand = new DepositCommand(0, username, amount);
			long commandId = commandManager.EnqueueCommand(depositCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "users")]
		public List<CommandNotification> GetPendingNotifications()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			List<CommandNotification> userNotifications = notificationHandler.GetUserNotifications(username);

			return userNotifications;
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "users")]
		public void Register()
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			RegistrationCommand registrationCommand = new RegistrationCommand(0, username);
			long commandId = commandManager.EnqueueCommand(registrationCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "users")]
		public void RequestLoan(double amount)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(0, username, amount);
			long commandId = commandManager.EnqueueCommand(requestLoanCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "users")]
		public void Withdraw(double amount)
		{
			string username = StringFormatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
			IClientServiceCallback callback = OperationContext.Current.GetCallbackChannel<IClientServiceCallback>();

			WithdrawCommand withdrawCommand = new WithdrawCommand(0, username, amount);
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
