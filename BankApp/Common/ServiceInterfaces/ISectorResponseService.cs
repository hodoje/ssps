using Common.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.ServiceInterfaces
{
	[ServiceContract]
	public interface ISectorResponseService
	{
		/// <summary>
		/// Returns an accept response to bank service.
		/// </summary>
		/// <param name="commandId">ID of accepted command.</param>
		/// <returns></returns>
		[OperationContract]
		CommandNotification Accept(long commandId);
		/// <summary>
		/// Returns a reject response to bank service.
		/// </summary>
		/// <param name="commandId">ID of rejected command.</param>
		/// <param name="reason">Reason why the command was rejected.</param>
		/// <returns></returns>
		[OperationContract]
		CommandNotification Reject(long commandId, string reason);
	}
}
