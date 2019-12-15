﻿using Common.Commanding;
using System.Collections.Generic;
using System.ServiceModel;

namespace Common.ServiceInterfaces
{
	[ServiceContract(CallbackContract = typeof(IClientServiceCallback))]
	public interface IUserService
	{
		[OperationContract]
		void Register();

		[OperationContract]
		void Withdraw(double amount);

		[OperationContract]
		void Deposit(double amount);

		[OperationContract]
		void RequestLoan(double amount);

		[OperationContract]
		List<CommandNotification> GetPendingNotifications();
	}
}
