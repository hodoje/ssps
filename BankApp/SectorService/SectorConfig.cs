using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectorService
{
	/// <summary>
	/// Loads app settings from App.config
	/// </summary>
	public static class SectorConfig
	{
		public const string AccountSectorServiceAddressConfigName = "AccountSectorServiceAddress";
		public const string LoanSectorServiceAddressConfigName = "LoanSectorServiceAddress";
		public const string TransactionSectorServiceAddressConfigName = "TransactionSectorServiceAddress";
		public const string AccountSectorServiceEndpointNameConfigName = "AccountSectorServiceEndpointName";
		public const string LoanSectorServiceEndpointNameConfigName = "LoanSectorServiceEndpointName";
		public const string TransactionSectorServiceEndpointNameConfigName = "TransactionSectorServiceEndpointName";
		public const string SectorResponseServiceAddressConfigName = "SectorResponseServiceAddress";
		public const string SectorResponseServiceEndpointConfigName = "SectorResponseServiceEndpoint";
		public const string AuditServiceAddressConfigName = "AuditServiceAddress";
		public const string AuditServiceEndpointNameConfigName = "AuditServiceEndpointName";
		public const string StartupConfirmationServiceAddressConfigName = "StartupConfirmationServiceAddress";
		public const string StartupConfirmationServiceEndpointNameConfigName = "StartupConfirmationServiceEndpointName";

		static SectorConfig()
		{
			AccountSectorServiceAddress = ConfigurationManager.AppSettings[AccountSectorServiceAddressConfigName];
			LoanSectorServiceAddress = ConfigurationManager.AppSettings[LoanSectorServiceAddressConfigName];
			TransactionSectorServiceAddress = ConfigurationManager.AppSettings[TransactionSectorServiceAddressConfigName];
			AccountSectorServiceEndpointName = ConfigurationManager.AppSettings[AccountSectorServiceEndpointNameConfigName];
			LoanSectorServiceEndpointName = ConfigurationManager.AppSettings[LoanSectorServiceEndpointNameConfigName];
			TransactionSectorServiceEndpointName = ConfigurationManager.AppSettings[TransactionSectorServiceEndpointNameConfigName];
			SectorResponseServiceAddress = ConfigurationManager.AppSettings[SectorResponseServiceAddressConfigName];
			SectorResponseServiceEndpoint = ConfigurationManager.AppSettings[SectorResponseServiceEndpointConfigName];
			AuditServiceAddress = ConfigurationManager.AppSettings[AuditServiceAddressConfigName];
			AuditServiceEndpointName = ConfigurationManager.AppSettings[AuditServiceEndpointNameConfigName];
			StartupConfirmationServiceAddress = ConfigurationManager.AppSettings[StartupConfirmationServiceAddressConfigName];
			StartupConfirmationServiceEndpointName = ConfigurationManager.AppSettings[StartupConfirmationServiceEndpointNameConfigName];
		}

		public static string AccountSectorServiceAddress { get; }
		public static string LoanSectorServiceAddress { get; }
		public static string TransactionSectorServiceAddress { get; }
		public static string AccountSectorServiceEndpointName { get; }
		public static string LoanSectorServiceEndpointName { get; }
		public static string TransactionSectorServiceEndpointName { get; }
		public static string SectorResponseServiceAddress { get; }
		public static string SectorResponseServiceEndpoint { get; }
		public static string AuditServiceAddress { get; }
		public static string AuditServiceEndpointName { get; }
		public static string StartupConfirmationServiceAddress { get; }
		public static string StartupConfirmationServiceEndpointName { get; }
	}
}
