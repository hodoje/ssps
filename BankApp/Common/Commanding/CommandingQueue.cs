using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Common.Commanding
{
	/// <summary>
	/// Class which represents commanding queue.
	/// </summary>
	public class CommandingQueue
	{
		private List<BaseCommand> commandingQueue;
		private ReaderWriterLockSlim locker;
		private TimeSpan timeoutPeriod;

		/// <summary>
		/// Initializes new instance of <see cref="CommandingQueue"/> class. 
		/// </summary>
		/// <param name="queueSize">Predefined queue size.</param>
		/// <param name="timeoutPeriodInSeconds">Timeout period in seconds.</param>
		public CommandingQueue(int queueSize, int timeoutPeriodInSeconds)
		{
			commandingQueue = new List<BaseCommand>(queueSize);
			locker = new ReaderWriterLockSlim();
			timeoutPeriod = new TimeSpan(0, 0, timeoutPeriodInSeconds);
		}

		/// <summary>
		/// Adds command to the queue.
		/// </summary>
		/// <param name="command">Command to add.</param>
		public void Enqueue(BaseCommand command)
		{
			locker.EnterWriteLock();

			commandingQueue.Add(command);

			locker.ExitWriteLock();
		}

		/// <summary>
		/// Returns commands in <b>FIFO</b> order.
		/// </summary>
		/// <returns>Returns command if there is any in the collection, otherwise null.</returns>
		public BaseCommand Dequeue()
		{
			BaseCommand returnCommand = null;

			locker.EnterWriteLock();

			if (commandingQueue.Count > 0)
			{
				returnCommand = commandingQueue[0];

				commandingQueue.RemoveAt(0);
			}

			locker.ExitWriteLock();

			return returnCommand;
		}

		/// <summary>
		/// Returns the command within queue with given command ID.
		/// </summary>
		/// <param name="commandId">Command id to search for.</param>
		/// <returns>Command if command exists in the queue, otherwise null.</returns>
		public BaseCommand GetCommandById(long commandId)
		{
			locker.EnterReadLock();

			BaseCommand command = commandingQueue.FirstOrDefault(x => x.CommandId == commandId);

			locker.ExitReadLock();

			return command;
		}

		/// <summary>
		/// Determines commands which timeout period has expired.
		/// </summary>
		/// <returns>Returns commands which are in timeout or empty list if there are none.</returns>
		public List<BaseCommand> ExpiredCommands()
		{
			List<BaseCommand> expiredCommands = new List<BaseCommand>();
			locker.EnterReadLock();

			DateTime dateTimeNow = DateTime.Now;

			foreach (BaseCommand command in commandingQueue)
			{
				if (command.CreationTime + timeoutPeriod <= dateTimeNow)
				{
					expiredCommands.Add(command);
				}
			}

			locker.ExitReadLock();

			return expiredCommands;
		}
	}
}
