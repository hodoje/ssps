using Common.Commanding;

namespace Common.ServiceInterfaces
{
	public interface IUserServiceCallback
	{
		void SendNotification(CommandNotification commandNotificaiton);
	}
}
