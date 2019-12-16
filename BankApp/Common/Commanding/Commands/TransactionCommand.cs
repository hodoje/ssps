using System;
using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used to represent deposit request initiated by user.
	/// </summary>
	[DataContract]
	public class TransactionCommand : BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="D"/> 
		/// </summary>
		public TransactionCommand()
		{

		}
		/// <summary>
		/// Initializes new instance of <see cref="TransactionCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command ID.</param>
		/// <param name="username">Username of user who requested deposit.</param>
		/// <param name="amount">Deposit amount.</param>
		public TransactionCommand(long commandId, string username, double amount, TransactionType transactionType) : base(commandId, username)
		{
			Amount = amount;
		}

		/// <summary>
		/// Requested deposit amount.
		/// </summary>
		[DataMember]
		public double Amount { get; private set; }

		/// <summary>
		/// Type of transaction.
		/// </summary>
		[DataMember]
		public TransactionType TransactionType { get; set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			TransactionCommand transactionCommand = obj as TransactionCommand;
			if (transactionCommand == null)
			{
				return false;
			}

			return base.Equals(obj) && Amount == transactionCommand.Amount && TransactionType == transactionCommand.TransactionType;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string StringifyCommand()
		{
			return $"{Username}: deposit requests with {Amount}$";
		}
	}
}
