using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.EventArguments
{
	public class LoginEventArgs : EventArgs
	{
		public UserType UserType { get; set; }

		public LoginEventArgs(UserType userType)
		{
			UserType = userType;
		}
	}
}
