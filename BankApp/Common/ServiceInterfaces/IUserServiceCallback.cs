using Common.Commanding;
using System.ServiceModel;

namespace Common.ServiceInterfaces
{
	public interface IUserServiceCallback
	{
		[OperationContract]
		void SendNotification(CommandNotification commandNotificaiton);
	}
}
