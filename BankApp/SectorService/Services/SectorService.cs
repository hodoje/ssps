using Common.Commanding;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SectorService.Services
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class SectorService : ISectorService
	{
		private SectorManager _sectorManager = new SectorManager();

		public void SendRequest(BaseCommand command, byte[] integrityCheck)
		{
			_sectorManager.EnqueueCommand(command);
		}
	}
}
