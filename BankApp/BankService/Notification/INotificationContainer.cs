using Common.Commanding;
using Common.ServiceInterfaces;
using System.Collections.Generic;

namespace BankService.Notification
{
	public interface INotificationContainer
	{
		void DeleteReceivedCommandNotification(long commandId);
		void CommandNotificationReceived(CommandNotification receivedCommandNotification);
		void AddExpectingNotificationId(string username, IUserServiceCallback userCallback, long commandId);
		List<CommandNotification> GetCommandNotificationsForUser(string username);
	}
}
