using Common.Commanding;

namespace BankService.CommandHandler
{
	/// <summary>
	/// Interface exposes methods used by unit which is used for command sending and notification receiving.
	/// </summary>
	public interface ICommandHandler
	{
		/// <summary>
		/// Indicates if there is space to add new command to handler.
		/// </summary>
		/// <returns><b>True</b> if there is enough space for new command, otherwise <b>false</b>.</returns>
		bool HasAvailableSpace();

		/// <summary>
		/// Sends command to command handler.
		/// </summary>
		/// <param name="command">Command to be sent.</param>
		void SendCommandToSector(BaseCommand command);

		/// <summary>
		/// Indicate command handler unit that command has been received.
		/// </summary>
		/// <param name="">Command notification received.</param>
		void CommandNotificationReceived(CommandNotification commandNotification);
	}
}
