using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;
using BankService.DatabaseManagement;

namespace BankService.CommandHandler
{
	/// <summary>
	/// Unit responsible for sending commands and receiving notifications from sectors.
	/// </summary>
	public class CommandHandler : ICommandHandler
	{
		private INotificationHost notificationHost;
		private IDatabaseManager<BaseCommand> databaseManager;
		private HashSet<long> commandsSent;
		private readonly int sectorSize;
		private object locker;

		/// <summary>
		/// Initializes new instance of <see cref="CommandHandler"/> class. 
		/// </summary>
		/// <param name="notificationHost">Notification host to notify for received command notification.</param>
		public CommandHandler(INotificationHost notificationHost, IDatabaseManager<BaseCommand> databaseManager)
		{
			this.notificationHost = notificationHost;
			this.databaseManager = databaseManager;
			this.sectorSize = 1;

			// open ClientProxy for Sector
			// open Service for command response

			locker = new object();
			commandsSent = new HashSet<long>(sectorSize);
		}

		/// <inheritdoc/>
		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			lock (locker)
			{
				if (commandsSent.Contains(commandNotification.ID))
				{
					ChangeCommandState(commandNotification.ID, CommandState.Executed);

					commandsSent.Remove(commandNotification.ID);
					notificationHost.CommandNotificationReceived(commandNotification);
				}
			}
		}

		private void ChangeCommandState(long id, CommandState state)
		{
			BaseCommand command = databaseManager.Get(id);
			command.State = state;

			databaseManager.Update(command);
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
		public void SendCommandToSector(BaseCommand command)
		{
			if (SendCommand(command))
			{
				ChangeCommandState(command.ID, CommandState.Sent);

				commandsSent.Add(command.ID);
			}

			//CommandNotificationReceived(new CommandNotification(command.ID));
		}

		private bool SendCommand(BaseCommand command)
		{
			return false;
		}
	}
}
