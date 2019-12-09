using Common.Commanding;
using Common.ServiceInterfaces;
using System.ServiceModel;
using BankService.CommandingManager;
using System.Collections.Generic;

namespace BankService
{
	/// <summary>
	/// Class represents banking service.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class BankingService : IUserService, IAdminService
	{
		private ICommandingManager commandManager;
		private object locker = new object();
		private long commandIdNumber = 0;

		public BankingService()
		{
			commandManager = new CommandingManager.CommandingManager();
		}

		public void Deposit(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			long newId = GetUniqueId();

			DepositCommand depositCommand = new DepositCommand(newId, username, amount);
			commandManager.EnqueueCommand(depositCommand);

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
		}

		public void Register(string username, string password)
		{
			// authorization and authentication
			long newId = GetUniqueId();

			RegistrationCommand registrationCommand = new RegistrationCommand(newId, username, password);
			commandManager.EnqueueCommand(registrationCommand);

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
		}

		public void RequestLoan(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			long newId = GetUniqueId();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(newId, username, amount);
			commandManager.EnqueueCommand(requestLoanCommand);

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
		}

		public void Withdraw(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			long newId = GetUniqueId();

			WithdrawCommand withdrawCommand = new WithdrawCommand(newId, username, amount);
			commandManager.EnqueueCommand(withdrawCommand);

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
		}

		public void CreateNewDatabase()
		{
			// authorization and authentication
			string username = null;

			commandManager.CreateDatabase();

			lock (locker)
			{
				commandIdNumber = 0;
			}

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
		}

		public void DeleteStaleCommands()
		{
			// authorization and authentication
			string username = null;

			commandManager.ClearStaleCommands();

			commandManager.RegisterClient(username, OperationContext.Current.GetCallbackChannel<IUserServiceCallback>());
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

		public List<CommandNotification> GetPendingNotifications()
		{
			string username = null;
			// authentication

			List<CommandNotification> userNotifications = null;

			// find notifications in notifications unit

			return userNotifications;
		}
	}
}
