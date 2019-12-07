﻿using System;
using System.Collections.Generic;
using Common.Commanding;
using System.Threading;
using System.Linq;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Unit responsible for database handling.
	/// </summary>
	internal class DatabaseManager : IDatabaseManager
	{
		private ReaderWriterLockSlim locker;
		private List<BaseCommand> commandsInDatabase;
		private IDataPersistence dataPersistance;

		/// <summary>
		/// Initializes new instance of <see cref="DatabaseManager"/> class. 
		/// </summary>
		/// <param name="dataPersistence">Unit used for data persistence.</param>
		public DatabaseManager(IDataPersistence dataPersistence)
		{
			this.dataPersistance = dataPersistence;

			locker = new ReaderWriterLockSlim();

			commandsInDatabase = new List<BaseCommand>();
		}

		/// <inheritdoc/>
		public List<BaseCommand> ReadAllCommands()
		{
			List<BaseCommand> commandsInDatabase;
			locker.EnterReadLock();
			
			commandsInDatabase = new List<BaseCommand>(this.commandsInDatabase);

			locker.ExitReadLock();

			return commandsInDatabase;
		}

		/// <inheritdoc/>
		public void RemoveCommand(long commandId)
		{
			locker.EnterReadLock();

			BaseCommand commandInDB = commandsInDatabase.FirstOrDefault(x => x.CommandId == commandId);

			locker.ExitReadLock();

			if (commandInDB == null)
			{
				Console.WriteLine($"[ERROR][DatabaseManager] Command with {commandId} does not exist in database.");

				//log ERROR
			}

			locker.EnterWriteLock();

			commandsInDatabase.Remove(commandInDB);
			dataPersistance.RemoveItem(commandId);

			locker.ExitWriteLock();

			// log successful remove
		}

		/// <inheritdoc/>
		public void SaveCommand(BaseCommand baseCommand)
		{
			locker.EnterWriteLock();

			if (commandsInDatabase.Exists(x => x.CommandId == baseCommand.CommandId))
			{
				locker.ExitReadLock();
				
				// log ERROR
				Console.WriteLine($"[DatabaseManager] Command with {baseCommand.CommandId} already exists in the database.");
				return;
			}

			commandsInDatabase.Add(baseCommand);
			dataPersistance.SaveItem(baseCommand);

			locker.ExitReadLock();
			// log successful add to DB
		}
	}
}