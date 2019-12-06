﻿using System;
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
	}
}
