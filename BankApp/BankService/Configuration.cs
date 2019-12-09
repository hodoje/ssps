using Common.Commanding;
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
		private static Configuration instance = new Configuration();

		private Configuration()
		{
			// todo add connection to sectors from configuration source
			Connections = new Dictionary<Type, ConnectionInfo>()
			{
				{ typeof(DepositCommand), new ConnectionInfo() },
				{ typeof(WithdrawCommand), new ConnectionInfo() },
				{ typeof(RequestLoanCommand), new ConnectionInfo() },
				{ typeof(RegistrationCommand), new ConnectionInfo() },
			};
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
