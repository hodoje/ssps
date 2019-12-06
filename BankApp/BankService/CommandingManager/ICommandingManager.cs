using Common.Command;
using System.Collections.Generic;

namespace BankService.CommandingManager
{
	/// <summary>
	/// Unit responsible for sending commands and response handling.
	/// </summary>
	public interface ICommandingProcessor
	{
		/// <summary>
		/// Enqueues command on the specific commanding queue.
		/// </summary>
		/// <param name="command"></param>
		void EnqueueCommand(Command command);

		/// <summary>
		/// Handles received command response.
		/// </summary>
		/// <param name="commandNotificatoin">Received command notification.</param>
		void ResponseReceived(CommandNotification commandNotification);

		/// <summary>
		/// Gets command which are in queue and ready to be sent.
		/// </summary>
		IEnumerable<Command> CommandsToSend { get; }

		/// <summary>
		/// Finds command in the queue and removes it from the queue.
		/// </summary>
		/// <param name="commandId">Unique commanding id.</param>
		/// <returns><b>True</b> if command is successfully deleted from the processor, otherwise <b>false</b>.</returns>
		bool CancelCommand(long commandId);
	}
}
