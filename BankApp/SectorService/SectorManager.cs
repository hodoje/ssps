﻿using Common.Commanding;
using Common.Communication;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SectorService
{
	/// <summary>
	/// Responsible for all the command processing and responding.
	/// </summary>
	public class SectorManager
	{
		#region Fields
		private CommandQueue _requestQueue;
		private CommandQueue _responseQueue;
		private WindowsClientProxy<ISectorResponseService> _responseProxy;
		private Task _processorTask;
		private Task _responderTask;
		#endregion

		#region Properties
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes an instance of <see cref="SectorManager" class./>
		/// </summary>
		public SectorManager()
		{
			_requestQueue = new CommandQueue(1, 3600);
			_responseQueue = new CommandQueue(1, 3600);
			_responseProxy = new WindowsClientProxy<ISectorResponseService>(SectorConfig.SectorResponseServiceAddress, SectorConfig.SectorResponseServiceEndpoint);
			_processorTask = new Task(ProcessCommands);
			_responderTask = new Task(SendResponses);

			_processorTask.Start();
			_responderTask.Start();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Enqueues a command for processing. Used by Sector service to enqueue a command recieved from Bank service.
		/// </summary>
		/// <param name="command">Command recieved from Bank service.</param>
		public void EnqueueCommand(BaseCommand command)
		{
			_requestQueue.Enqueue(command);
		}

		/// <summary>
		/// Confirms a given command to Bank service.
		/// </summary>
		/// <param name="commandId">Id of a command that has been successfully executed.</param>
		private void AcceptRequest(long commandId)
		{
			_responseProxy.Proxy.Accept(commandId);
		}

		/// <summary>
		/// Rejects a given command to Bank service.
		/// </summary>
		/// <param name="commandId">Id of a command that has been rejected.</param>
		/// <param name="reason">Reason why a command has been rejected.</param>
		private void RejectRequest(long commandId, string reason)
		{
			_responseProxy.Proxy.Reject(commandId, reason);
		}
		
		/// <summary>
		/// Processes arrived requests and puts them in response queue.
		/// </summary>
		private void ProcessCommands()
		{
			Random acceptOrRejectRandomizer = new Random();

			while (true)
			{
				BaseCommand command = _requestQueue.Dequeue();
				if (command.TimedOut)
				{
					command.Status = CommandNotificationStatus.Rejected;					
				}
				else
				{
					int isAccepted = acceptOrRejectRandomizer.Next(0, 1);
					if(isAccepted == 1)
					{
						command.Status = CommandNotificationStatus.Confirmed;
					}
					else
					{
						command.Status = CommandNotificationStatus.Rejected;
					}
				}
				_responseQueue.Enqueue(command);
			}
		}

		/// <summary>
		/// Takes commands from response queue and sends according responses to Bank service.
		/// </summary>
		private void SendResponses()
		{
			while (true)
			{
				BaseCommand command = _responseQueue.Dequeue();
				switch (command.Status)
				{
					case CommandNotificationStatus.Confirmed:
						AcceptRequest(command.CommandId);
						break;
					case CommandNotificationStatus.Rejected:
						RejectRequest(command.CommandId, "Request timed out.");
						break;
					case CommandNotificationStatus.None:
						RejectRequest(command.CommandId, "Failed to process request.");
						break;
					default:
						RejectRequest(command.CommandId, "Invalid request.");
						break;
				}
			}
		}
		#endregion
	}
}
