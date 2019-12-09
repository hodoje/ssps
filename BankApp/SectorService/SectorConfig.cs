using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorService
{
	public static class SectorConfig
	{
		public const string AccountSectorServiceAddressConfigName = "AccountSectorServiceAddress";
		public const string LoanSectorServiceAddressConfigName = "LoanSectorServiceAddress";
		public const string TransactionSectorServiceAddressConfigName = "TransactionSectorServiceAddress";
		public const string AccountSectorServiceEndpointNameConfigName = "AccountSectorServiceEndpointName";
		public const string LoanSectorServiceEndpointNameConfigName = "LoanSectorServiceEndpointName";
		public const string TransactionSectorServiceEndpointNameConfigName = "TransactionSectorServiceEndpointName";

		static SectorConfig()
		{
			AccountSectorServiceAddress = ConfigurationManager.AppSettings[AccountSectorServiceAddressConfigName];
			LoanSectorServiceAddress = ConfigurationManager.AppSettings[LoanSectorServiceAddressConfigName];
			TransactionSectorServiceAddress = ConfigurationManager.AppSettings[TransactionSectorServiceAddressConfigName];
			AccountSectorServiceEndpointName = ConfigurationManager.AppSettings[AccountSectorServiceEndpointNameConfigName];
			LoanSectorServiceEndpointName = ConfigurationManager.AppSettings[LoanSectorServiceEndpointNameConfigName];
			TransactionSectorServiceEndpointName = ConfigurationManager.AppSettings[TransactionSectorServiceEndpointNameConfigName];
	    }

		public static string AccountSectorServiceAddress { get; }
		public static string LoanSectorServiceAddress { get; }
		public static string TransactionSectorServiceAddress { get; }
		public static string AccountSectorServiceEndpointName { get; }
		public static string LoanSectorServiceEndpointName { get; }
		public static string TransactionSectorServiceEndpointName { get; }
	}
}
