using Common.Commanding;
using Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankService
{
	public struct ConnectionInfo
	{
		EndpointAddress EndpointAddess { get; set; }
		NetTcpBinding NetTcpBinding { get; set; }
	}

	/// <summary>
	/// Loads app settings from App.config
	/// </summary>
	public static class BankServiceConfig
	{
		public const string BankServiceAddressConfigName = "BankServiceAddress";
		public const string UserServiceEndpointNameConfigName = "UserServiceEndpointName";
		public const string SectorResponseServiceAddressConfigName = "SectorResponseServiceAddress";
		public const string SectorResponseServiceEndpointConfigName = "SectorResponseServiceEndpoint";
		public const string AuditServiceAddressConfigName = "AuditServiceAddress";
		public const string AuditServiceEndpointNameConfigName = "AuditServiceEndpointName";
		public const string AllSectorNamesConfigName = "AllSectorNames";
		public const string SectorExeFilenameConfigName = "SectorExeFilename";
		public const string StartupConfirmationServiceAddressConfigName = "StartupConfirmationServiceAddress";
		public const string StartupConfirmationServiceEndpointNameConfigName = "StartupConfirmationServiceEndpointName";

		static BankServiceConfig()
		{
			BankServiceAddress = ConfigurationManager.AppSettings[BankServiceAddressConfigName];
			UserServiceEndpointName = ConfigurationManager.AppSettings[UserServiceEndpointNameConfigName];
			SectorResponseServiceAddress = ConfigurationManager.AppSettings[SectorResponseServiceAddressConfigName];
			SectorResponseServiceEndpoint = ConfigurationManager.AppSettings[SectorResponseServiceEndpointConfigName];
			AuditServiceAddress = ConfigurationManager.AppSettings[AuditServiceAddressConfigName];
			AuditServiceEndpointName = ConfigurationManager.AppSettings[AuditServiceEndpointNameConfigName];
			AllSectorNames = ConfigurationManager.AppSettings[AllSectorNamesConfigName].Split(',');
			string rawSectorExeFilename = ConfigurationManager.AppSettings[SectorExeFilenameConfigName];
			string workingDirectory = Environment.CurrentDirectory;
			string _solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
			SectorExeFilename = rawSectorExeFilename.Replace("{AppDir}", _solutionDirectory);
			StartupConfirmationServiceAddress = ConfigurationManager.AppSettings[StartupConfirmationServiceAddressConfigName];
			StartupConfirmationServiceEndpointName = ConfigurationManager.AppSettings[StartupConfirmationServiceEndpointNameConfigName];

			// ili SectorAddiotnalConfig ne znam gde si planirao da drzis informaciju o endpoint-u servisa sa strane hosta i servisa sa strane sektora
			Connections = AllSectorNames.ToDictionary(x => x, x => new ConnectionInfo());
		}

		private static Dictionary<string, SectorAdditionalConfig> GetSectorsConfig(string sectorsConfigJson)
		{
			Dictionary<string, SectorAdditionalConfig> result = new Dictionary<string, SectorAdditionalConfig>();
			JObject sectorsConfigJObject = JsonConvert.DeserializeObject<JObject>(sectorsConfigJson);
			var sectors = sectorsConfigJObject["sectors"];

			foreach (var child in sectors.Children())
			{
				SectorConfigs.Add((child as JProperty).Name, child.First.ToObject<SectorAdditionalConfig>());
			}

			return result;
		}

		public static string BankServiceAddress { get; }
		public static string UserServiceEndpointName { get; }
		public static string SectorResponseServiceAddress { get; }
		public static string SectorResponseServiceEndpoint { get; }
		public static string AuditServiceAddress { get; }
		public static string AuditServiceEndpointName { get; }
		public static Dictionary<string, ConnectionInfo> Connections { get; set; }
		public static string[] AllSectorNames { get; }
		public static Dictionary<string, SectorAdditionalConfig> SectorConfigs { get; }
		public static string SectorExeFilename { get; }
		public static string StartupConfirmationServiceAddress { get; }
		public static string StartupConfirmationServiceEndpointName { get; }
	}
}
