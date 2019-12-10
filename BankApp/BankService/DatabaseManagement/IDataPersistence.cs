using System.Collections.Generic;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Used to expose methods for unit which is responsible for saving data to permanent storage.
	/// </summary>
	public interface IDataPersistence<T>
	{
		/// <summary>
		/// Save entity to permanent storage.
		/// </summary>
		/// <param name="item">Command to save.</param>
		void AddEntity(T item);

		/// <summary>
		/// Removes entity from permanent storage by entities id.
		/// </summary>
		/// <param name="entityId">ID of the entity to be removed.</param>
		void RemoveEntity(long commandId);

		/// <summary>
		/// Read all entities from permanent storage.
		/// </summary>
		/// <returns>List of entities in permanent storage.</returns>
		List<T> ReadAllEntities();
	}
}
