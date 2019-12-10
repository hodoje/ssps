using Common.Commanding;
using Common.ServiceInterfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BankService.Notification
{
	public class NotificationContainer : INotificationContainer
	{
		private ConcurrentDictionary<string, NotificationInformation> pendingUserNotifications;

		public NotificationContainer()
		{
			pendingUserNotifications = new ConcurrentDictionary<string, NotificationInformation>(LoadNotificationsFromDatabase().ToDictionary(x => x.Username, x => x));
		}

		public void AddExpectingNotificationId(string username, IUserServiceCallback userCallback, long commandId)
		{
			NotificationInformation userNotificationInfo = pendingUserNotifications.GetOrAdd(username, (x) => { return new NotificationInformation(username, userCallback); });
			userNotificationInfo.PendingNotifications.TryAdd(commandId, commandId);

			//todo save to db
		}

		public void CommandNotificationReceived(CommandNotification receivedCommandNotification)
		{
			foreach (NotificationInformation notificationinfo in pendingUserNotifications.Values)
			{
				long notificationId;
				if (notificationinfo.PendingNotifications.TryRemove(receivedCommandNotification.ID, out notificationId))
				{
					//todo update db

					receivedCommandNotification.CommandState = CommandState.Executed;
					notificationinfo.ReadyNotifications.TryAdd(receivedCommandNotification.ID, receivedCommandNotification);
				}
			}
		}

		public void DeleteReceivedCommandNotification(long commandId)
		{
			foreach (NotificationInformation notificationinfo in pendingUserNotifications.Values)
			{
				CommandNotification commandNotification;
				if (notificationinfo.ReadyNotifications.TryRemove(commandId, out commandNotification))
				{
					//todo remove from db

					return;
				}
			}
		}

		public List<CommandNotification> GetCommandNotificationsForUser(string username)
		{
			NotificationInformation userNotification;
			pendingUserNotifications.TryRemove(username, out userNotification);
			List<CommandNotification> userNotifications = userNotification?.ReadyNotifications.Values.ToList();

			return userNotifications;
		}

		private List<NotificationInformation> LoadNotificationsFromDatabase()
		{
			return new List<NotificationInformation>();
		}
	}
}
