using System;
using System.Threading.Tasks;
using System.Threading;
using Common.Commanding;
using BankService.CommandHandler;
using System.Collections.Concurrent;

namespace BankService.CommandingHost
{
	public class CommandingHost : ICommandingHost, INotificationHost, IDisposable
	{
		private CancellationTokenSource cancellationToken = new CancellationTokenSource();
		private ICommandHandler commandHandler;
		private CommandingQueue commandingQueue;
		private ConcurrentQueue<CommandNotification> responseQueue;
		private AutoResetEvent sendingSynchronization = new AutoResetEvent(false);

		public CommandingHost(CommandingQueue commandingQueue, ConcurrentQueue<CommandNotification> responseQueue)
		{
			commandHandler = new CommandHandler.CommandHandler(this);

			this.responseQueue = responseQueue;
			this.commandingQueue = commandingQueue;
		}

		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			responseQueue.Enqueue(commandNotification);

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
			// Get commands from commanding queue and send it to command handler
			Task listenQueueTask = new Task(WorkerThread);
			listenQueueTask.Start();
		}

		public void Stop()
		{
			cancellationToken?.Cancel();
			sendingSynchronization?.Set();
		}

		private void WorkerThread()
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				BaseCommand commandToSend = commandingQueue.Dequeue();

				if (!commandHandler.HasAvailableSpace())
				{
					sendingSynchronization.WaitOne();
				}

				if (!cancellationToken.IsCancellationRequested)
				{
					commandHandler.SendCommand(commandToSend);
				}
			}
		}
	}
}
