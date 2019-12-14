using Common.CertificateManagement;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;

namespace Common.Communication
{
	/// <summary>
	/// Represents client proxy for given interface with certificate management.
	/// </summary>
	/// <typeparam name="T">Interface which will be used for proxy.</typeparam>
	public class CertificateClientProxy<T> : ChannelFactory<T>, IDisposable where T : class
	{
		private T _proxy;

		/// <summary>
		/// Initializes a new instance of <see cref="CertificateClientProxy<T> class."/>
		/// </summary>
		/// <param name="serviceAddress">Service address.</param>
		/// <param name="serviceEndpointName">Service endpoint name.</param>
		public CertificateClientProxy(string serviceAddress, string serviceEndpointName)
			: base(SetUpBinding(), SetUpEndpoint(serviceAddress, serviceEndpointName))
		{
			string cltCertCN = ParseName(WindowsIdentity.GetCurrent().Name);

			Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
			Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

			X509Certificate2 certificate;
			if (CertificateStorageReader.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN, out certificate))
			{
				this.Credentials.ClientCertificate.Certificate = certificate;
			}
			else
			{
				throw new ArgumentException("Service will not be initializes since it has no valid certificate!");
			}
		}

		/// <summary>
		/// Proxy used for communication with service.
		/// </summary>
		public T Proxy
		{
			get
			{
				if (_proxy == null)
				{
					_proxy = this.CreateChannel();
				}
				return _proxy;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_proxy = null;
		}

		private static EndpointAddress SetUpEndpoint(string serviceAddress, string serviceEndpointName)
		{
			return new EndpointAddress($"{serviceAddress}/{serviceEndpointName}");
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

		private static NetTcpBinding SetUpBinding()
		{
			var binding = new NetTcpBinding(SecurityMode.Transport);
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;

			return binding;
		}
	}
}
