using System.Collections.Generic;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Interface which exposes methods of a database management unit.
	/// </summary>
	public interface IDatabaseManager<T>
	{
		/// <summary>
		/// Used for concurrent reading all entities which are currently in database.
		/// </summary>
		/// <returns>Entities which are in database.</returns>
		List<T> ReadAllEntities();

		/// <summary>
		/// Used for concurrent saving given entity to the database.
		/// </summary>
		/// <param name="baseCommand">Entity to be saved to database.</param>
		void SaveEntity(T entity);

		/// <summary>
		/// Used to concurrent removing entity by id from database.
		/// </summary>
		/// <param name="entityId">Entity id to remove from database.</param>
		void RemoveEntity(long entityId);

		/// <summary>
		/// Set new data persistence object.
		/// </summary>
		/// <param name="dataPersistence">New data persistence object.</param>
		void LoadNewDataPersitenceUnit(IDataPersistence<T> dataPersistence);
	}
}
