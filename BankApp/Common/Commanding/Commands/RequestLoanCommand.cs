using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used to represent loan request initiated by user.
	/// </summary>
	[DataContract]
	public class RequestLoanCommand : BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="RequestLoanCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command ID.</param>
		/// <param name="username">Username of user who requested loan.</param>
		/// <param name="amount">Loan amount.</param>
		public RequestLoanCommand(long commandId, string username, double amount) : base(commandId)
		{
			Username = username;
			Amount = amount;
		}

		/// <summary>
		/// Requested loan amount.
		/// </summary>
		[DataMember]
		public double Amount { get; private set; }

		/// <summary>
		/// Username of the user who requested loan.
		/// </summary>
		[DataMember]
		public string Username { get; private set; }
	}
}
