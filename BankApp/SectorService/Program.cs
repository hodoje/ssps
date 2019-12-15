using Common.Model;
using SectorService.ServiceHosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SectorService
{
	class Program
	{
		static void Main(string[] args)
		{
			string sectorType = args[0];
			Console.WriteLine(sectorType);

			try
			{
				if (SectorConfig.SectorsConfigs.TryGetValue(sectorType, out SectorAdditionalConfig sectorAdditionalConfig))
				{
					var sectorHost = new SectorServiceServiceHost(sectorAdditionalConfig);
					sectorHost.OpenService();

					//WindowsClientProxy<IStartupConfirmationService> startupProxy = new WindowsClientProxy<IStartupConfirmationService>(
					//	SectorConfig.StartupConfirmationServiceAddress, SectorConfig.StartupConfirmationServiceEndpointName);
					//startupProxy.Proxy.ConfirmStartup(sectorType);
				}
				else
				{
					throw new Exception($"{sectorType} ServiceHost does not exist.");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Reporting exception to Bank service.");
				Console.WriteLine("Shutting down...");
				throw new FaultException(e.Message);
			}

			Console.Read();
		}
	}
}
