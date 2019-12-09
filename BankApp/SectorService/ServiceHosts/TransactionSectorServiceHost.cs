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
	public class TransactionSectorServiceHost : IDisposable
	{
		private readonly string _sectorServiceAddress;
		private readonly string _sectorServiceEndpointName;
		private readonly ServiceHost _sectorServiceServiceHost;
		private readonly NetTcpBinding _binding;

		public TransactionSectorServiceHost()
		{
			_sectorServiceAddress = SectorConfig.AccountSectorServiceAddress;
			_sectorServiceEndpointName = SectorConfig.AccountSectorServiceEndpointName;
			_binding = SetUpBinding();
			_sectorServiceServiceHost = new ServiceHost(typeof(SectorService.Services.SectorService));
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
				Console.WriteLine($"TransactionSectorServiceHost failed to open with an error: {ex.Message}");
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
				Console.WriteLine($"TransactionSectorServiceHost failed to close with an error: {ex.Message}");
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
