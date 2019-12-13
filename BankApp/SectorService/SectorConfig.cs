using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		public const string SectorResponseServiceAddressConfigName = "SectorResponseServiceAddress";
		public const string SectorResponseServiceEndpointConfigName = "SectorResponseServiceEndpoint";
		public const string AuditServiceAddressConfigName = "AuditServiceAddress";
		public const string AuditServiceEndpointNameConfigName = "AuditServiceEndpointName";
		public const string StartupConfirmationServiceAddressConfigName = "StartupConfirmationServiceAddress";
		public const string StartupConfirmationServiceEndpointNameConfigName = "StartupConfirmationServiceEndpointName";
		public const string AllSectorNamesConfigName = "AllSectorNames";
		public const string SectorsConfigName = "Sectors";

		static SectorConfig()
		{
			SectorResponseServiceAddress = ConfigurationManager.AppSettings[SectorResponseServiceAddressConfigName];
			SectorResponseServiceEndpoint = ConfigurationManager.AppSettings[SectorResponseServiceEndpointConfigName];
			AuditServiceAddress = ConfigurationManager.AppSettings[AuditServiceAddressConfigName];
			AuditServiceEndpointName = ConfigurationManager.AppSettings[AuditServiceEndpointNameConfigName];
			StartupConfirmationServiceAddress = ConfigurationManager.AppSettings[StartupConfirmationServiceAddressConfigName];
			StartupConfirmationServiceEndpointName = ConfigurationManager.AppSettings[StartupConfirmationServiceEndpointNameConfigName];
			AllSectorNames = ConfigurationManager.AppSettings[AllSectorNamesConfigName].Split(',');

			string sectorsConfigJson = ConfigurationManager.AppSettings[SectorsConfigName];
			SectorConfigs = GetSectorsConfig(sectorsConfigJson);
		}

		private static Dictionary<string, AddressEndpointPair> GetSectorsConfig(string sectorsConfigJson)
		{
			Dictionary<string, AddressEndpointPair> result = new Dictionary<string, AddressEndpointPair>();
			JObject sectorsConfigJObject = JsonConvert.DeserializeObject<JObject>(sectorsConfigJson);
			var sectors = sectorsConfigJObject["sectors"];
			
			foreach (var child in sectors.Children())
			{
				SectorConfigs.Add((child as JProperty).Name, child.First.ToObject<AddressEndpointPair>());
			}

			return result;
		}

		public static string SectorResponseServiceAddress { get; }
		public static string SectorResponseServiceEndpoint { get; }
		public static string AuditServiceAddress { get; }
		public static string AuditServiceEndpointName { get; }
		public static string StartupConfirmationServiceAddress { get; }
		public static string StartupConfirmationServiceEndpointName { get; }
		public static string[] AllSectorNames { get; }
		public static Dictionary<string, AddressEndpointPair> SectorConfigs { get; }
	}
}
