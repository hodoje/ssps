using Common.Model;
using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Represents command notification.
	/// </summary>
	[DataContract]
	public class CommandNotification : IdentifiedObject
	{
		/// <summary>
		/// Initializes new instance of <see cref="CommandNotification"/> class.
		/// </summary>
		/// <param name="commandId"></param>
		public CommandNotification(long commandId) : base(commandId)
		{
			CommandStatus = CommandNotificationStatus.None;
		}

		/// <summary>
		/// Represents the current state of the command.
		/// </summary>
		[DataMember]
		public CommandState CommandState { get; set; }

		/// <summary>
		/// Represents the current command status.
		/// </summary>
		[DataMember]
		public CommandNotificationStatus CommandStatus { get; set; }

		/// <summary>
		/// Any kind of additional information or message why command is rejected or accepted. 
		/// </summary>
		[DataMember]
		public string Information { get; set; }
	}
}
