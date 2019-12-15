using Client.Model;
using Common.Commanding;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public class BankServiceCallbackObject : IClientServiceCallback
	{
		private Action<CommandNotification> _callback;

		public BankServiceCallbackObject(Action<CommandNotification> callback)
		{
			_callback = callback;
		}

		public void SendNotification(CommandNotification commandNotification)
		{
			_callback(commandNotification);
		}
	}
}
