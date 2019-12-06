using Common.Commanding;
using Common.ServiceInterfaces;

namespace BankService
{
	/// <summary>
	/// Class represents banking service.
	/// </summary>
	public class BankingService : IUserService, IAdminService
	{
		private CommandingManager.CommandingManager commandManager;
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
		}

		public void Register(string username, string password)
		{
			// authorization and authentication
			long newId = GetUniqueId();

			RegistrationCommand registrationCommand = new RegistrationCommand(newId, username, password);
			commandManager.EnqueueCommand(registrationCommand);
		}

		public void RequestLoan(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			long newId = GetUniqueId();

			RequestLoanCommand requestLoanCommand = new RequestLoanCommand(newId, username, amount);
			commandManager.EnqueueCommand(requestLoanCommand);
		}

		public void Withdraw(double amount)
		{
			// authorization and authentication
			string username = null; // get username
			long newId = GetUniqueId();

			WithdrawCommand withdrawCommand = new WithdrawCommand(newId, username, amount);
			commandManager.EnqueueCommand(withdrawCommand);
		}

		private long GetUniqueId()
		{
			long newId;
			lock (locker)
			{
				newId = ++commandIdNumber;
			}

			return newId;
		}
	}
}
