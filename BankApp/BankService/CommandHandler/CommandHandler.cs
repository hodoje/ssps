using System;
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
	/// <summary>
	/// Unit responsible for sending commands and receiving notifications from sectors.
	/// </summary>
	public class CommandHandler : ICommandHandler, ISectorResponseService
	{
		private object locker;
		private readonly int sectorSize;

		private HashSet<long> commandsSent;

		private IDatabaseManager<BaseCommand> databaseManager;
		private INotificationHost notificationHost;

		private WindowsClientProxy<ISectorService> sectoreServiceProxy;

		private IAudit auditService;

		/// <summary>
		/// Initializes new instance of <see cref="CommandHandler"/> class. 
		/// </summary>
		/// <param name="notificationHost">Notification host to notify for received command notification.</param>
		public CommandHandler(string sectorType, IAudit auditService, INotificationHost notificationHost, IDatabaseManager<BaseCommand> databaseManager, int sectorQueueSize)
		{
			this.auditService = auditService;
			this.notificationHost = notificationHost;
			this.databaseManager = databaseManager;
			this.sectorSize = sectorQueueSize;

			ConnectionInfo ci = BankServiceConfig.Connections[sectorType];
			sectoreServiceProxy = new WindowsClientProxy<ISectorService>(ci.Address, ci.EndpointName);

			locker = new object();
			commandsSent = LoadSentCommands();
		}

		/// <inheritdoc/>
		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			lock (locker)
			{
				if (commandsSent.Contains(commandNotification.ID))
				{
					auditService.Log("CommandHandler", $"Response received for command with {commandNotification.ID} id.");
					commandsSent.Remove(commandNotification.ID);
					notificationHost.CommandNotificationReceived(commandNotification);
				}
				else
				{
					auditService.Log("CommandHandler", $"Unexpected response received for command with {commandNotification.ID} id.");
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
			bool commandSent = SendCommand(command);
			if (commandSent)
			{
				ChangeCommandState(command.ID, CommandState.Sent);

				commandsSent.Add(command.ID);

				auditService.Log("CommandHandler", $"Command ({command.ToString()}) sent to sector.");
			}
			else
			{
				auditService.Log("CommandHandler", $"Command ({command.ToString()}) not sent to sector.", System.Diagnostics.EventLogEntryType.Error);
			}

			//CommandNotificationReceived(new CommandNotification(command.ID));

			return commandSent;
		}

		private void ChangeCommandState(long id, CommandState state)
		{
			BaseCommand command = databaseManager.Get(id);
			command.State = state;

			// log to audit : command changed state
			auditService.Log("CommandHandler", $"Command ({command.ToString()}) changed state to sent.");

			databaseManager.Update(command);
		}

		private HashSet<long> LoadSentCommands()
		{
			return new HashSet<long>(databaseManager.Find(x => x.State == CommandState.Sent).Select(x => x.ID));
		}

		private bool SendCommand(BaseCommand command)
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
		public void Accept(long commandId)
		{
			CommandNotification cn = new CommandNotification(commandId);
			CommandNotificationReceived(cn);
		}

		public void Reject(long commandId, string reason)
		{
			CommandNotification cn = new CommandNotification(commandId);
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
	}
}
