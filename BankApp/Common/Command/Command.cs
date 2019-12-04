namespace Common.Command
{
	/// <summary>
	/// Class used to represent user command.
	/// </summary>
	public class Command
	{
		/// <summary>
		/// Initializes new instance of <see cref="Command"/> class.
		/// </summary>
		/// <param name="commandId">Unique command id.</param>
		/// <param name="commandType">Command type.</param>
		public Command(long commandId, CommandType commandType)
		{
			CommandId = commandId;
			CommandType = commandType;
		}
		/// <summary>
		/// Unique command id.
		/// </summary>
		public long CommandId { get; private set; }
		/// <summary>
		/// Command type.
		/// </summary>
		public CommandType CommandType { get; set; }
	}
}
