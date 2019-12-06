using System.ServiceModel;

namespace Common.ServiceInterfaces
{
	[ServiceContract]
	public interface IUserService
	{
		[OperationContract]
		void Register(string username, string password);

		[OperationContract]
		void Withdraw(double amount);

		[OperationContract]
		void Deposit(double amount);

		[OperationContract]
		void RequestLoan(double amount);
	}
}
