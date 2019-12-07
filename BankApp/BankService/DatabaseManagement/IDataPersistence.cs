using Common.Commanding;

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
		/// Removes entity from database by entities id.
		/// </summary>
		/// <param name="entityId">ID of the command to be removed.</param>
		void RemoveItem(long commandId);
	}
}
