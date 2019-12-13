using System;
using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used to represent withdraw request initiated by user.
	/// </summary>
	[DataContract]
	public class WithdrawCommand : BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="WithdrawCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command ID.</param>
		/// <param name="username">Username of user who requested withdraw.</param>
		/// <param name="amount">Withdraw amount.</param>
		public WithdrawCommand(long commandId, string username, double amount) : base(commandId, username)
		{
			Amount = amount;
		}

		/// <summary>
		/// Requested withdraw amount.
		/// </summary>
		[DataMember]
		public double Amount { get; private set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			WithdrawCommand withdrawCommand = obj as WithdrawCommand;
			if (withdrawCommand == null)
			{
				return false;
			}

			return base.Equals(obj) && Amount == withdrawCommand.Amount;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
