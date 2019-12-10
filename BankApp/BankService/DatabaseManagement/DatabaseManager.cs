using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Common.Model;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Unit responsible for database handling.
	/// </summary>
	internal class DatabaseManager<T> : IDatabaseManager<T>, IDisposable
		where T : IdentifiedObject
	{
		private ReaderWriterLockSlim locker;
		private List<T> commandsInDatabase;
		private IDataPersistence<T> dataPersistence;

		/// <summary>
		/// Initializes new instance of <see cref="DatabaseManager"/> class. 
		/// </summary>
		/// <param name="dataPersistence">Unit used for data persistence.</param>
		public DatabaseManager(IDataPersistence<T> dataPersistence)
		{
			this.dataPersistence = dataPersistence;

			locker = new ReaderWriterLockSlim();

			commandsInDatabase = new List<T>();
		}

		/// <inheritdoc/>
		public List<T> ReadAllEntities()
		{
			List<T> commandsInDatabase;
			locker.EnterReadLock();
			
			commandsInDatabase = new List<T>(this.commandsInDatabase);

			locker.ExitReadLock();

			return commandsInDatabase;
		}

		/// <inheritdoc/>
		public void RemoveEntity(long commandId)
		{
			locker.EnterReadLock();

			T commandInDB = commandsInDatabase.FirstOrDefault(x => x.ID == commandId);

			locker.ExitReadLock();

			if (commandInDB == null)
			{
				Console.WriteLine($"[ERROR][DatabaseManager] Command with {commandId} does not exist in database.");

				//log ERROR
			}

			locker.EnterWriteLock();

			commandsInDatabase.Remove(commandInDB);
			dataPersistence?.RemoveEntity(commandId);

			locker.ExitWriteLock();

			// log successful remove
		}

		/// <inheritdoc/>
		public void SaveEntity(T baseCommand)
		{
			locker.EnterWriteLock();

			if (commandsInDatabase.Exists(x => x.ID == baseCommand.ID))
			{
				locker.ExitReadLock();
				
				// log ERROR
				Console.WriteLine($"[DatabaseManager] Command with {baseCommand.ID} already exists in the database.");
				return;
			}

			commandsInDatabase.Add(baseCommand);
			dataPersistence?.AddEntity(baseCommand);

			locker.ExitReadLock();
			// log successful add to DB
		}

		/// <inheritdoc/>
		public void LoadNewDataPersitenceUnit(IDataPersistence<T> dataPersistence)
		{
			locker.EnterWriteLock();

			this.dataPersistence = dataPersistence;

			if (commandsInDatabase.Count > 0)
			{
				commandsInDatabase.ForEach(x => dataPersistence.AddEntity(x));
			}

			locker.ExitWriteLock();
		}

		public void Dispose()
		{
			locker.Dispose();
			((IDisposable)dataPersistence).Dispose();
			commandsInDatabase.Clear();
		}
	}
}
