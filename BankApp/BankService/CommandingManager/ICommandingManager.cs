using Common.Commanding;
using System.Collections.Generic;

namespace BankService.CommandingManager
{
	/// <summary>
	/// Unit responsible for sending commands and response handling.
	/// </summary>
	public interface ICommandingManager
	{
		/// <summary>
		/// Enqueues command on the specific commanding queue.
		/// </summary>
		/// <param name="command"></param>
		void EnqueueCommand(BaseCommand command);

		/// <summary>
		/// Finds command in the queue and removes it from the queue.
		/// </summary>
		/// <param name="commandId">Unique commanding id.</param>
		/// <returns><b>True</b> if command is successfully deleted from the processor, otherwise <b>false</b>.</returns>
		bool CancelCommand(long commandId);

		/// <summary>
		/// Creates new database.
		/// </summary>
		void CreateDatabase();

		/// <summary>
		/// Deletes commands which are timed out.
		/// </summary>
		void ClearStaleCommands();
	}
}
