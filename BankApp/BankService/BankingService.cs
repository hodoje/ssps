using Common.Commanding;
using Common.ServiceInterfaces;
using System.ServiceModel;
using BankService.CommandingManager;
using System.Collections.Generic;
using System.Collections.Concurrent;
using BankService.Notification;

namespace BankService
{
	/// <summary>
	/// Class represents banking service.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class BankingService : IUserService, IAdminService
	{
		private long commandIdNumber = 0;
		private ICommandingManager commandManager;
		private object locker = new object();
		private INotificationHandler notificationHandler;

		private ConcurrentQueue<CommandNotification> responseQueue;
		public BankingService()
		{
			responseQueue = new ConcurrentQueue<CommandNotification>();
			notificationHandler = new NotificationHandler(responseQueue, new NotificationContainer());
			commandManager = new CommandingManager.CommandingManager(responseQueue);
		}

		public void CreateNewDatabase()
		{
			// authorization and authentication
			string username = null;
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			commandManager.CreateDatabase();

			lock (locker)
			{
				commandIdNumber = 0;
			}
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
			string username = null; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();
			long newId = GetUniqueId();

			DepositCommand depositCommand = new DepositCommand(newId, username, amount);
			commandManager.EnqueueCommand(depositCommand);

			notificationHandler.RegisterCommand(username, callback, newId);
		}

		public List<CommandNotification> GetPendingNotifications()
		{
			string username = null;
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			// authentication

			List<CommandNotification> userNotifications = null;

			// find notifications in notifications unit

			return userNotifications;
		}

		public void Register(string username, string password)
		{
			// authorization and authentication
			long newId = GetUniqueId();
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();

			RegistrationCommand registrationCommand = new RegistrationCommand(newId, username, password);
			commandManager.EnqueueCommand(registrationCommand);

			notificationHandler.RegisterCommand(username, callback, newId);

		}

		public void RequestLoan(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();
			long newId = GetUniqueId();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(newId, username, amount);
			commandManager.EnqueueCommand(requestLoanCommand);

			notificationHandler.RegisterCommand(username, callback, newId);
		}

		public void Withdraw(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			IUserServiceCallback callback = OperationContext.Current.GetCallbackChannel<IUserServiceCallback>();
			long newId = GetUniqueId();

			WithdrawCommand withdrawCommand = new WithdrawCommand(newId, username, amount);
			commandManager.EnqueueCommand(withdrawCommand);

			notificationHandler.RegisterCommand(username, callback, newId);
		}
		private long GetUniqueId()
		{
			// TODO, get biggest ID from DB

			long newId;
			lock (locker)
			{
				newId = ++commandIdNumber;
			}

			return newId;
		}
	}
}
