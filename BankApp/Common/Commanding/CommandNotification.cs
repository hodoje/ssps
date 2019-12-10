using Common.Model;

namespace Common.Commanding
{
	/// <summary>
	/// Represents command notification.
	/// </summary>
	public class CommandNotification : IdentifiedObject
	{
		/// <summary>
		/// Initializes new instance of <see cref="CommandNotification"/> class.
		/// </summary>
		/// <param name="commandId"></param>
		public CommandNotification(long commandId) : base(commandId)
		{
		}

		/// <summary>
		/// Represents the current state of the command.
		/// </summary>
		public CommandState CommandState { get; set; }

		/// <summary>
		/// Represents the current command status.
		/// </summary>
		public CommandNotificationStatus CommandStatus { get; set; }

		/// <summary>
		/// Any kind of additional information or message why command is rejected or accepted. 
		/// </summary>
		public string Information { get; set; }
	}
}
