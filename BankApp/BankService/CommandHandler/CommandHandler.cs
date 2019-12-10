using System;
using System.Collections.Generic;
using Common.Commanding;
using BankService.CommandingHost;

namespace BankService.CommandHandler
{
	/// <summary>
	/// Unit responsible for sending commands and receiving notifications from sectors.
	/// </summary>
	public class CommandHandler : ICommandHandler
	{
		private INotificationHost notificationHost;
		private HashSet<long> commandsSent;
		private object locker;

		/// <summary>
		/// Initializes new instance of <see cref="CommandHandler"/> class. 
		/// </summary>
		/// <param name="notificationHost">Notification host to notify for received command notification.</param>
		public CommandHandler(INotificationHost notificationHost)
		{
			this.notificationHost = notificationHost;

			// open ClientProxy for Sector
			// open Service for command response

			locker = new object();
			commandsSent = new HashSet<long>(10);
		}

		/// <inheritdoc/>
		public void CommandNotificationReceived(CommandNotification commandNotification)
		{
			lock (locker)
			{
				if (commandsSent.Contains(commandNotification.ID))
				{
					commandsSent.Remove(commandNotification.ID);
					notificationHost.CommandNotificationReceived(commandNotification);
				}
			}
		}

		/// <inheritdoc/>
		public bool HasAvailableSpace()
		{
			bool hasSpace;
			lock (locker)
			{
				hasSpace = commandsSent.Count == 10;
			}

			return !hasSpace;
		}

		/// <inheritdoc/>
		public void SendCommand(BaseCommand command)
		{
			// send command via ClientProxy
			throw new NotImplementedException();
		}
	}
}
