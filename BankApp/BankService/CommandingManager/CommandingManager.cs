using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Commanding;
using BankService.CommandingHost;
using System.Collections.Concurrent;
using System.Threading;

namespace BankService.CommandingManager
{
	public class CommandingManager : ICommandingManager
	{
		private static readonly int queueSize;
		private static readonly int timeoutPeriod;

		private CancellationTokenSource cancellationToken;
		private List<ICommandingHost> commandingHosts;

		private Dictionary<Type, CommandingQueue> commandToQueueMapper;
		private ConcurrentQueue<CommandNotification> responseQueue;

		static CommandingManager()
		{
			//TODO read from configuration
			queueSize = 5;
			timeoutPeriod = 3;
		}

		public CommandingManager()
		{
			cancellationToken = new CancellationTokenSource();
			responseQueue = new ConcurrentQueue<CommandNotification>();

			List<Type> supportedCommands = new List<Type>(4)
			{
				typeof(DepositCommand), typeof(WithdrawCommand), typeof(RequestLoanCommand), typeof(RegistrationCommand)
			};

			commandingHosts = new List<ICommandingHost>(supportedCommands.Count);

			AddCommandingHostByCommand(commandToQueueMapper, supportedCommands);

			Task listenWorker = new Task(ListenForCommandNotifications);
			listenWorker.Start();
		}

		public bool CancelCommand(long commandId)
		{
			foreach (CommandingQueue commandingQueue in commandToQueueMapper.Values)
			{
				if (commandingQueue.RemoveCommandById(commandId))
				{
					return true;
				}
			}

			return false;
		}

		public void EnqueueCommand(BaseCommand command)
		{
			commandToQueueMapper[command.GetType()].Enqueue(command);

			//log, save to db
		}

		private void AddCommandingHostByCommand(Dictionary<Type, CommandingQueue> commandToQueueMapper, List<Type> commandTypes)
		{
			foreach (Type commandType in commandTypes)
			{
				CommandingQueue newQueue = new CommandingQueue(queueSize, timeoutPeriod);
				CommandingHost.CommandingHost newHost = new CommandingHost.CommandingHost(newQueue, responseQueue);
				commandingHosts.Add(newHost);

				commandToQueueMapper.Add(commandType, newQueue);
				newHost.Start();
			}
		}

		private void ListenForCommandNotifications()
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				CommandNotification notification;
				if (!responseQueue.TryDequeue(out notification))
				{
					Thread.Sleep(300);
					continue;
				}

				// todo send notification to unit responsible for user notification
			}
		}
	}
}
