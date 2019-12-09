using Common.Commanding;
using Common.ServiceInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BankService
{
	public class NotificationHandler : INotificationHandler, IDisposable
	{
		private CancellationTokenSource cancellationToken;
		private ConcurrentQueue<CommandNotification> notificationQueue;

		public NotificationHandler(ConcurrentQueue<CommandNotification> notificationQueue)
		{
			this.notificationQueue = notificationQueue;

			cancellationToken = new CancellationTokenSource();
		}

		public void Dispose()
		{
			Stop();
		}

		public List<CommandNotification> GetUserNotifications(string key)
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			Task listenWorker = new Task(ListenForCommandNotifications);
			listenWorker.Start();
		}

		public void Stop()
		{
			cancellationToken.Cancel();
		}

		public void TryRegisterUserForNotifications(string key, IUserServiceCallback userCallback)
		{
			
		}

		private void ListenForCommandNotifications()
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				CommandNotification notification;
				if (!notificationQueue.TryDequeue(out notification))
				{
					Thread.Sleep(2000);
					continue;
				}

				// todo notification
			}
		}

		public void RegisterCommand(string key, long commandId)
		{
			
		}
	}
}
