﻿using Common.Commanding;
using Common.ServiceInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BankService.Notification;

namespace BankService
{
	public class NotificationHandler : INotificationHandler, IDisposable
	{
		private CancellationTokenSource cancellationToken;
		private INotificationContainer notificationContainer;
		private ConcurrentQueue<CommandNotification> notificationQueue;

		public NotificationHandler(ConcurrentQueue<CommandNotification> notificationQueue, INotificationContainer notificationContainer)
		{
			this.notificationQueue = notificationQueue;
			this.notificationContainer = notificationContainer;

			cancellationToken = new CancellationTokenSource();

			Task listenQueueTask = new Task(ListenForCommandNotifications);
			listenQueueTask.Start();
		}

		public void Dispose()
		{
			Stop();
		}

		public List<CommandNotification> GetUserNotifications(string username)
		{
			List<CommandNotification> notifications = notificationContainer.GetCommandNotificationsForUser(username);

			return notifications?? new List<CommandNotification>();
		}

		public void RegisterCommand(string username, IUserServiceCallback userCallback, long commandId)
		{
			notificationContainer.AddExpectingNotificationId(username, userCallback, commandId);
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

				string username;
				IUserServiceCallback callback = notificationContainer.CommandNotificationReceived(notification, out username);

				if (SendNotificationToClient(callback, notification))
				{
					notificationContainer.CommandNotificationSent(notification.ID);
				}
				else
				{
					// set callback on null
					notificationContainer.DefaultUsersCallback(username);
				}
			}
		}

		private bool SendNotificationToClient(IUserServiceCallback callback, CommandNotification commandNotification)
		{
			try
			{
				callback.SendNotification(commandNotification);

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
