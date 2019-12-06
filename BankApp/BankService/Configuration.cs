using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace BankService
{
	public struct ConnectionInfo
	{
		EndpointAddress EndpointAddess { get; set; }
		NetTcpBinding NetTcpBinding { get; set; }
	}
	internal class Configuration
	{
		private static Configuration instance;

		static Configuration()
		{
			instance = new Configuration();
			// todo Connections
		}

		public static Configuration Instance
		{
			get
			{
				return instance;
			}
		}

		public Dictionary<Type, ConnectionInfo> Connections { get; set; }
	}
}
