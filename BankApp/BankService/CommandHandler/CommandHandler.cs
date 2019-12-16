﻿using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using BankService.DatabaseManagement;
using System.Linq;
using Common.Communication;
using Common.ServiceInterfaces;
using System.ServiceModel;
using System.Net.Security;

namespace BankService.CommandHandler
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	/// <summary>
	/// Unit responsible for sending commands and receiving notifications from sectors.
	/// </summary>
	public class CommandHandler : ICommandHandler, ISectorResponseService
	{
		private object locker;
		private readonly int sectorSize;
		private Dictionary<long, BaseCommand> commandsSent;
		private WindowsClientProxy<ISectorService> sectoreServiceProxy;
		private IAudit auditService;
		private INotificationHost notificationHost;

		/// <summary>
		/// Initializes new instance of <see cref="CommandHandler"/> class. 
		/// </summary>
		/// <param name="notificationHost">Notification host to notify for received command notification.</param>
		public CommandHandler(string sectorType, IAudit auditService, INotificationHost notificationHost, int sectorQueueSize, List<BaseCommand> sentCommands)
		{
			this.auditService = auditService;
			this.notificationHost = notificationHost;
			this.sectorSize = sectorQueueSize;

			ConnectionInfo ci = BankServiceConfig.Connections[sectorType];
			sectoreServiceProxy = new WindowsClientProxy<ISectorService>(ci.Address, ci.EndpointName);

			ServiceHost sectorResponseServiceHost = new ServiceHost(this);
			sectorResponseServiceHost.AddServiceEndpoint(
				typeof(ISectorResponseService), 
				SetUpBindingForSectoreResponse(), 
				$"{ci.SectorResponseAddress}/{ci.SectorResponseEndpoint}");
			try
			{
				sectorResponseServiceHost.Open();
				Console.WriteLine($"{sectorType.ToUpper()} sector response service opened.");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}

			locker = new object();
			this.commandsSent = sentCommands.ToDictionary(x => x.ID, x => x);
		}

		/// <inheritdoc/>
		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			lock (locker)
			{
				if (commandsSent.ContainsKey(commandNotification.ID))
				{
					commandsSent.Remove(commandNotification.ID);

					auditService.Log("CommandHandler", $"Response received for command with {commandNotification.ID} id.");

					notificationHost.CommandNotificationReceived(commandNotification);
				}
				else
				{
					auditService.Log("CommandHandler", $"Unexpected response received for command with {commandNotification.ID} id.", System.Diagnostics.EventLogEntryType.Warning);
				}
			}
		}

		/// <inheritdoc/>
		public bool HasAvailableSpace()
		{
			bool hasSpace;
			lock (locker)
			{
				hasSpace = commandsSent.Count == sectorSize;
			}

			return !hasSpace;
		}

		/// <inheritdoc/>
		public bool SendCommandToSector(BaseCommand command)
		{
			bool commandSent = TrySendCommand(command);
			if (commandSent)
			{
				// remember sent command in sake of response handling
				commandsSent.Add(command.ID, command);

				auditService.Log("CommandHandler", $"Command ({command.ToString()}) sent to sector.");
			}
			else
			{
				auditService.Log("CommandHandler", $"Command ({command.ToString()}) not sent to sector.", System.Diagnostics.EventLogEntryType.Error);
			}

			return commandSent;
		}

		private bool TrySendCommand(BaseCommand command)
		{
			try
			{
				// Send command to sector
				// TODO: DO INTEGRITY CHECK
				sectoreServiceProxy.Proxy.SendRequest(command, new byte[1]);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#region ISectoreResponse
		public void Accept(long commandId, string information)
		{
			CommandNotification cn = new CommandNotification(commandId, CommandNotificationStatus.Confirmed);
			cn.Information = information;     
			CommandNotificationReceived(cn);
		}

		public void Reject(long commandId, string reason)
		{
			CommandNotification cn = new CommandNotification(commandId, CommandNotificationStatus.Rejected);
			cn.Information = reason;
			CommandNotificationReceived(cn);
		}
		#endregion

		private NetTcpBinding SetUpBindingForStartupConfirmation()
		{
			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.Sign;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			return binding;
		}

		private NetTcpBinding SetUpBindingForSectoreResponse()
		{
			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.Sign;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			return binding;
		}
	}
}
