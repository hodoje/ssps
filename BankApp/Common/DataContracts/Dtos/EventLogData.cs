using Common.DataContracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataContracts.Dtos
{
	/// <summary>
	///     Represents an item for EventLog
	/// </summary>
	[DataContract]
	public class EventLogData
	{
		public EventLogData()
		{
		}

		public EventLogData(string bankName, string accountName, string logMessage, EventLogEntryType eventLogEntryType)
		{
			BankName = !string.IsNullOrWhiteSpace(bankName)
				? bankName
				: throw new ArgumentNullException(nameof(bankName));
			AccountName = !string.IsNullOrWhiteSpace(accountName)
				? accountName
				: throw new ArgumentNullException(nameof(accountName));
			LogMessage = !string.IsNullOrWhiteSpace(logMessage)
				? logMessage
				: throw new ArgumentNullException(nameof(logMessage));
			EventLogType = Enum.IsDefined(typeof(EventLogEntryType), eventLogEntryType)
				? eventLogEntryType
				: throw new InvalidEventLogTypeException();
		}

		[DataMember]
		public string BankName { get; set; }

		[DataMember]
		public string AccountName { get; set; }

		[DataMember]
		public string LogMessage { get; set; }

		[DataMember]
		public EventLogEntryType EventLogType { get; set; }
	}
}
