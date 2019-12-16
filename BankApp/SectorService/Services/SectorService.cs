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
		private SectorManager _sectorManager;

		public SectorService(string sectorType, int sectorQueueSize, int sectorQueueTimeoutPeriodInSeconds)
		{
			_sectorManager = new SectorManager(sectorType, sectorQueueSize, sectorQueueTimeoutPeriodInSeconds);
		}

		public void SendRequest(BaseCommand command, byte[] integrityCheck)
		{
			_sectorManager.EnqueueCommand(command);
		}
	}
}
