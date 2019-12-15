using System;
using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used to represent loan request initiated by user.
	/// </summary>
	[DataContract]
	public class RequestLoanCommand : BaseCommand
	{
		public RequestLoanCommand()
		{

		}

		/// <summary>
		/// Initializes new instance of <see cref="RequestLoanCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command ID.</param>
		/// <param name="username">Username of user who requested loan.</param>
		/// <param name="amount">Loan amount.</param>
		public RequestLoanCommand(long commandId, string username, double amount) : base(commandId, username)
		{
			Amount = amount;
		}

		/// <summary>
		/// Requested loan amount.
		/// </summary>
		[DataMember]
		public double Amount { get; private set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			RequestLoanCommand requestLoanCommand = obj as RequestLoanCommand;
			if (requestLoanCommand == null)
			{
				return false;
			}

			return base.Equals(obj) && Amount == requestLoanCommand.Amount;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string StringifyCommand()
		{
			return $"{Username} requests a loan of {Amount}$";
		}
	}
}
