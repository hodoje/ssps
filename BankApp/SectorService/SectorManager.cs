using Common.Commanding;
using Common.Communication;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SectorService
{
	public class SectorManager
	{
		private CommandQueue _requestQueue;
		private CommandQueue _responseQueue;
		// TODO: set up proxy
		private WindowsClientProxy<ISectorResponseService> _responseProxy;

		public SectorManager()
		{
			_requestQueue = new CommandQueue(1, 3600);
			_responseQueue = new CommandQueue(1, 3600);
		}

		public void EnqueueCommand(BaseCommand command)
		{
			_requestQueue.Enqueue(command);
		}

		private void AcceptRequest()
		{
			//_responseProxy.Proxy.Accept();
		}

		private void RejectRequest()
		{
			//_responseProxy.Proxy.Reject();
		}
	}
}
