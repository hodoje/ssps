using Common.Commanding;
using Common.ServiceInterfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BankService
{
	public interface INotificationHandler
	{
		List<CommandNotification> GetUserNotifications(string key);
		void RegisterCommand(string username, IUserServiceCallback userCallback, long commandId);
		void Start();
		void Stop();
	}

	public class NotificationInformation
	{
		public NotificationInformation(string username, IUserServiceCallback userCallback)
		{
			Username = username;
			ReadyNotifications = new ConcurrentDictionary<long, CommandNotification>();
			PendingNotifications = new ConcurrentDictionary<long, long>();
			UserCallback = userCallback;
		}

		public ConcurrentDictionary<long, CommandNotification> ReadyNotifications { get; private set; }
		public ConcurrentDictionary<long, long> PendingNotifications { get; private set; }
		public IUserServiceCallback UserCallback { get; private set; }
		public string Username { get; private set; }
	}
}
