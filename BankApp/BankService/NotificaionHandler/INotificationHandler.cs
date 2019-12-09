using Common.Commanding;
using Common.ServiceInterfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BankService
{
	public interface INotificationHandler
	{
		List<CommandNotification> GetUserNotifications(string key);
		void RegisterCommand(string key, long commandId);
		void Start();
		void Stop();
		void TryRegisterUserForNotifications(string key, IUserServiceCallback userCallback);
	}

	internal class NotificationInformation
	{
		public NotificationInformation(IUserServiceCallback userCallback)
		{
			Notifications = new ConcurrentDictionary<long, CommandNotification>();
			UserCallback = userCallback;
		}

		public ConcurrentDictionary<long, CommandNotification> Notifications { get; private set; }
		public IUserServiceCallback UserCallback { get; private set; }
	}
}
