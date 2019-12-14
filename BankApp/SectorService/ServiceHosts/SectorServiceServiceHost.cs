using Common.Model;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SectorService.ServiceHosts
{
	public class SectorServiceServiceHost : IDisposable
	{
		private readonly string _sectorServiceAddress;
		private readonly string _sectorServiceEndpointName;
		private readonly ServiceHost _sectorServiceServiceHost;
		private readonly NetTcpBinding _binding;

		public SectorServiceServiceHost(SectorAdditionalConfig sectorConfig)
		{
			_sectorServiceAddress = sectorConfig.Address;
			_sectorServiceEndpointName = sectorConfig.EndpointName;
			_binding = SetUpBinding();
			Services.SectorService sectorService = new Services.SectorService(SectorConfig.SectorQueueSize, SectorConfig.SectorQueueTimeoutInSeconds);
			_sectorServiceServiceHost = new ServiceHost(sectorService);
			_sectorServiceServiceHost.AddServiceEndpoint(typeof(ISectorService), _binding,
				$"{_sectorServiceAddress}/{_sectorServiceEndpointName}");
		}

		public void OpenService()
		{
			try
			{
				_sectorServiceServiceHost.Open();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{_sectorServiceEndpointName.ToUpper()} ServiceHost failed to open with an error: {ex.Message}");
			}
		}

		public void CloseService()
		{
			try
			{
				_sectorServiceServiceHost.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{_sectorServiceEndpointName.ToUpper()} ServiceHost failed to close with an error: {ex.Message}");
			}
		}

		private NetTcpBinding SetUpBinding()
		{
			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.Sign;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			return binding;
		}

		public void Dispose()
		{
			(_sectorServiceServiceHost as IDisposable).Dispose();
		}
	}
}
