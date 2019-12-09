using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
	public class Notification : BindableBase
	{
		private string _message;
		private string _displayedInfo;

		public string Message
		{
			get { return _message; }
			set { SetField(ref _message, value); }
		}

		public string DisplayedInfo
		{
			get { return _displayedInfo; }
			set { SetField(ref _displayedInfo, value); }
		}

		public Notification(string message)
		{
			_message = message;
			DisplayedInfo = $"{Message}{Environment.NewLine}STATUS: SUCCESSFUL";
		}
	}
}
