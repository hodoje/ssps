using Common.Commanding;
using System.Collections.Generic;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Used to expose methods for unit which is responsible for saving data to permanent storage.
	/// </summary>
	public interface IDataPersistence
	{
		/// <summary>
		/// Save command to permanent storage.
		/// </summary>
		/// <param name="item">Command to save.</param>
		void SaveItem(BaseCommand item);

		/// <summary>
		/// Removes entity from permanent storage by entities id.
		/// </summary>
		/// <param name="entityId">ID of the command to be removed.</param>
		void RemoveItem(long commandId);

		/// <summary>
		/// Read all commands from permanent storage.
		/// </summary>
		/// <returns>List of commands in permanent storage.</returns>
		List<BaseCommand> ReadAllCommands();
	}
}
