using Common.Commanding;
using Common.ServiceInterfaces;
using System.Collections.Generic;

namespace BankService.Notification
{
	public interface INotificationContainer
	{
		void DefaultUsersCallback(string username);
		void CommandNotificationSent(long commandId);

		void AddExpectingNotificationId(string username, IUserServiceCallback userCallback, long commandId);

		List<CommandNotification> GetCommandNotificationsForUser(string username);

		IUserServiceCallback CommandNotificationReceived(CommandNotification receivedCommandNotification, out string username);
	}
}
