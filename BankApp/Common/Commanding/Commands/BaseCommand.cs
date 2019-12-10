using Common.Model;
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
	public abstract class BaseCommand : IdentifiedObject
	{
		/// <summary>
		/// Initializes new instance of <see cref="BaseCommand"/> class.
		/// </summary>
		/// <param name="commandId">Unique command id.</param>
		public BaseCommand(long commandId) : base(commandId)
		{
			CreationTime = DateTime.Now;
			Status = CommandNotificationStatus.None;
		}

		/// <summary>
		/// Defines when was the command created.
		/// </summary>
		[DataMember]
		public DateTime CreationTime { get; private set; }

		/// <summary>
		/// Indicates if command is in timeout.
		/// </summary>
		public bool TimedOut { get; set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			BaseCommand command = obj as BaseCommand;

			if (obj == null)
			{
				return false;
			}

			return base.Equals(obj) && CreationTime == command.CreationTime;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.GetType().Name} : {ID}";
		}

		/// <summary>
		/// Command status determined by Sector.
		/// </summary>
		[DataMember]
		public CommandNotificationStatus Status { get; set; }
	}
}
