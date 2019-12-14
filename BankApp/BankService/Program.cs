using Common.CertificateManagement;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace BankService
{
	class Program
	{
		static void Main(string[] args)
		{
			NetTcpBinding binding = new NetTcpBinding();
			string address = "net.tcp://localhost:9999/Receiver";
			BankingService bankingService = new BankingService();

			ServiceHost host = new ServiceHost(bankingService);
			host.AddServiceEndpoint(typeof(IUserService), binding, address);

			///Custom validation mode enables creation of a custom validator - CustomCertificateValidator
			//host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;

			///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
			//host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

			///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
			//host.Credentials.ServiceCertificate.Certificate = CertificateStorageReader.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

			//string srvCertCN = ParseName(WindowsIdentity.GetCurrent().Name);

			host.Open();

			Console.ReadLine();
		}

		private static string ParseName(string winLogonName)
		{
			string[] parts = new string[] { };

			if (winLogonName.Contains("@"))
			{
				///UPN format
				parts = winLogonName.Split('@');
				return parts[0];
			}
			else if (winLogonName.Contains("\\"))
			{
				/// SPN format
				parts = winLogonName.Split('\\');
				return parts[1];
			}
			else
			{
				return winLogonName;
			}
		}
	}
}
