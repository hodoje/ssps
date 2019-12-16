using System;
using System.Threading.Tasks;
using System.Threading;
using Common.Commanding;
using BankService.CommandHandler;
using System.Collections.Concurrent;
using BankService.DatabaseManagement;
using Common.Communication;
using Common.ServiceInterfaces;

namespace BankService.CommandingHost
{
	public class CommandingHost : ICommandingHost, INotificationHost, IDisposable
	{
		private string hostName;

		private CancellationTokenSource cancellationToken = new CancellationTokenSource();
		private AutoResetEvent sendingSynchronization = new AutoResetEvent(false);

		private CommandQueue commandingQueue;
		private ConcurrentQueue<CommandNotification> responseQueue;

		private ICommandHandler commandHandler;
		private IDatabaseManager<BaseCommand> databaseManager;

		private IAudit auditService;

		public CommandingHost(string sectorType, IAudit auditService, CommandQueue commandingQueue, ConcurrentQueue<CommandNotification> responseQueue, ConnectionInfo connectionInfo, IDatabaseManager<BaseCommand> databaseManager, string hostName)
		{
			commandHandler = new CommandHandler.CommandHandler(sectorType, auditService, this, databaseManager, BankServiceConfig.SectorQueueSize);

			this.auditService = auditService;
			this.responseQueue = responseQueue;
			this.commandingQueue = commandingQueue;
			this.hostName = hostName;
			this.databaseManager = databaseManager;
		}

		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			responseQueue.Enqueue(commandNotification);

			BaseCommand command = databaseManager.Get(commandNotification.ID);
			if (command != null)
			{
				command.State = CommandState.Executed;
				databaseManager.Update(command);

				auditService.Log(command.ToString(), "Changed state to executed!");
			}

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

				// Queue might be in disposing procedure.
				if (commandToSend == null)
				{
					continue;
				}

				auditService.Log("CommandingHost", $"Dequeued {commandToSend.GetType().Name}(id = {commandToSend.ID}) command.");

				if (!commandHandler.HasAvailableSpace())
				{
					// If there is not enough space on host (sector is full) wait for response to be received.
					sendingSynchronization.WaitOne();
				}

				if (cancellationToken.IsCancellationRequested)
				{
					// When awoken, check if cancellation token was canceled (object disposing).
					return;
				}

				if (!commandHandler.SendCommandToSector(commandToSend))
				{
                    // If command handler couldn't sent the command, requeue the command.
					commandingQueue.Enqueue(commandToSend);
				}
			}
		}
	}
}
