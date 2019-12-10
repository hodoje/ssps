using System;
using System.Threading.Tasks;
using System.Threading;
using Common.Commanding;
using BankService.CommandHandler;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;

namespace BankService.CommandingHost
{
	public class CommandingHost : ICommandingHost, INotificationHost, IDisposable
	{
		private string hostName;
		private CancellationTokenSource cancellationToken = new CancellationTokenSource();
		private ICommandHandler commandHandler;
		private IDatabaseManager<BaseCommand> databaseManager;
		private CommandQueue commandingQueue;
		private ConcurrentQueue<CommandNotification> responseQueue;
		private AutoResetEvent sendingSynchronization = new AutoResetEvent(false);

		public CommandingHost(CommandQueue commandingQueue, ConcurrentQueue<CommandNotification> responseQueue, ConnectionInfo connectionInfo, IDatabaseManager<BaseCommand> databaseManager, string hostName)
		{
			// todo create Client for Sector with connecitonInfo

			commandHandler = new CommandHandler.CommandHandler(this);

			this.responseQueue = responseQueue;
			this.commandingQueue = commandingQueue;
			this.hostName = hostName;
			this.databaseManager = databaseManager;
		}

		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			responseQueue.Enqueue(commandNotification);
			databaseManager.RemoveEntity(commandNotification.ID);

			// Awake WorkerThread because there is enough command space in Commanding Handler.
			sendingSynchronization.Set();
		}

		public void Dispose()
		{
			Stop();
			commandingQueue.Dispose();
		}

		public void Start()
		{
			// Get commands from commanding queue and send it to Commanding Handler.
			Task listenQueueTask = new Task(WorkerThread);
			listenQueueTask.Start();
		}

		public void Stop()
		{
			// Cancel WorkerThread
			cancellationToken?.Cancel();

			// Wake up WorkerThread if it is waiting for Commanding Handler to get enough space.
			sendingSynchronization?.Set();
		}

		private void WorkerThread()
		{
			Console.WriteLine($"[CommandingHost] {hostName} started...");
			while (!cancellationToken.IsCancellationRequested)
			{
				BaseCommand commandToSend = commandingQueue.Dequeue();

				// Queue might be disposing procedure.
				if (commandToSend == null)
				{
					continue;
				}

				if (!commandHandler.HasAvailableSpace())
				{
					// If there is not enough space on host (sector is full) wait for response to be received.
					sendingSynchronization.WaitOne();
				}

				// When awoken, check if cancellation token was canceled (object disposing).
				if (!cancellationToken.IsCancellationRequested)
				{
					commandHandler.SendCommand(commandToSend);
				}
			}
		}
	}
}
