using Common.Commanding;
using Common.Model;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.ServiceInterfaces
{
	[ServiceContract(CallbackContract = typeof(IClientServiceCallback))]
	public interface IUserService
	{
		[OperationContract]
		void Register();

		[OperationContract]
		void Withdraw(double amount);

		[OperationContract]
		void Deposit(double amount);

		[OperationContract]
		void RequestLoan(double amount, int months);

		[OperationContract]
		List<BankAccount> GetMyBankAccounts();

		[OperationContract]
		List<CommandNotification> GetPendingNotifications();
	}
}
