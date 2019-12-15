using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using BankService.DatabaseManagement;
using System.Linq;
using Common.Communication;
using Common.ServiceInterfaces;

namespace BankService.CommandHandler
{
	/// <summary>
	/// Unit responsible for sending commands and receiving notifications from sectors.
	/// </summary>
	public class CommandHandler : ICommandHandler
	{
		private object locker;
		private readonly int sectorSize;

		private HashSet<long> commandsSent;

		private IDatabaseManager<BaseCommand> databaseManager;
		private INotificationHost notificationHost;

		private IAudit auditService;

		/// <summary>
		/// Initializes new instance of <see cref="CommandHandler"/> class. 
		/// </summary>
		/// <param name="notificationHost">Notification host to notify for received command notification.</param>
		public CommandHandler(IAudit auditService, INotificationHost notificationHost, IDatabaseManager<BaseCommand> databaseManager, int sectorQueueSize)
		{
			this.auditService = auditService;
			this.notificationHost = notificationHost;
			this.databaseManager = databaseManager;
			this.sectorSize = sectorQueueSize;

			// open ClientProxy for Sector
			// open Service for command response

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
			if (command == null)
			{
				return;
			}

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
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
