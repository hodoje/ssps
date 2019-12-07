using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

			host.Open();

			Console.ReadLine();
		}
	}
}
