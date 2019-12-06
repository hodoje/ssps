using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used to represent deposit request initiated by user.
	/// </summary>
	[DataContract]
	public class DepositCommand : BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="DepositCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command ID.</param>
		/// <param name="username">Username of user who requested deposit.</param>
		/// <param name="amount">Deposit amount.</param>
		public DepositCommand(long commandId, string username, double amount) : base(commandId)
		{
			Username = username;
			Amount = amount;
		}

		/// <summary>
		/// Requested deposit amount.
		/// </summary>
		[DataMember]
		public double Amount { get; private set; }

		/// <summary>
		/// Username of the user who requested deposit.
		/// </summary>
		[DataMember]
		public string Username { get; private set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			DepositCommand depositCommand = obj as DepositCommand;
			if (depositCommand == null)
			{
				return false;
			}

			return base.Equals(obj) && Amount == depositCommand.Amount && Username == depositCommand.Username;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
