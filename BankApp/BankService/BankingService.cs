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

		private ConcurrentQueue<CommandNotification> responseQueue;
		public BankingService()
		{
			connectionString = "TEST";

			InitializesObjects();

			responseQueue = new ConcurrentQueue<CommandNotification>();
			notificationHandler = new NotificationHandler(responseQueue, new NotificationContainer(ServiceLocator.GetService<IRepository<CommandNotification>>()));

			commandManager = new CommandingManager.CommandingManager(responseQueue);
			commandManager.CreateDatabase();
		}

		public void CreateNewDatabase()
		{
			// authorization and authentication
			string username = null;
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			commandManager.CreateDatabase();
		}

		public void DeleteStaleCommands()
		{
			// authorization and authentication
			string username = null;
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			commandManager.ClearStaleCommands();
		}

		public void Deposit(double amount)
		{
			// authorization and authentication
			string username = "dummy"; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			DepositCommand depositCommand = new DepositCommand(0, username, amount);
			long commandId = commandManager.EnqueueCommand(depositCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		public List<CommandNotification> GetPendingNotifications()
		{
			string username = null;

			List<CommandNotification> userNotifications = notificationHandler.GetUserNotifications(username);

			return userNotifications;
		}

		public void Register()
		{
			// authorization and authentication
			string username = null;
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			RegistrationCommand registrationCommand = new RegistrationCommand(0, username);
			long commandId = commandManager.EnqueueCommand(registrationCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);

		}

		public void RequestLoan(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(0, username, amount);
			long commandId = commandManager.EnqueueCommand(requestLoanCommand);

			notificationHandler.RegisterCommand(username, callback, commandId);
		}

		public void Withdraw(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

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
