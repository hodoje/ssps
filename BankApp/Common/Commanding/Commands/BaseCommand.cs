using System;
using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Class used to represent user command.
	/// </summary>
	[DataContract]
	[KnownType(typeof(DepositCommand))]
	[KnownType(typeof(WithdrawCommand))]
	[KnownType(typeof(RequestLoanCommand))]
	[KnownType(typeof(RegistrationCommand))]
	public abstract class BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="BaseCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command id.</param>
		public BaseCommand(long commandId)
		{
			CommandId = commandId;
			CreationTime = DateTime.Now;
		}

		/// <summary>
		/// Defines when was the command created.
		/// </summary>
		[DataMember]
		public DateTime CreationTime { get; private set; }

		/// <summary>
		/// Unique command id.
		/// </summary>
		[DataMember]
		public long CommandId { get; private set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			BaseCommand command = obj as BaseCommand;

			if (obj == null)
			{
				return false;
			}

			return CreationTime == command.CreationTime && CommandId == command.CommandId;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.GetType().Name} : {CommandId}";
		}
	}
}
