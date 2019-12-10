using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	/// <summary>
	/// Loads app settings from App.config
	/// </summary>
	public static class ClientConfig
	{
		public const string BankServiceAddressConfigName = "BankServiceAddress";
		public const string BankServiceEndpointNameConfigName = "BankServiceEndpointName";

		static ClientConfig()
		{
			BankServiceAddress = ConfigurationManager.AppSettings[BankServiceAddressConfigName];
			BankServiceEndpointName = ConfigurationManager.AppSettings[BankServiceEndpointNameConfigName];
		}

		public static string BankServiceAddress { get; }
		public static string BankServiceEndpointName { get; }
	}
}
