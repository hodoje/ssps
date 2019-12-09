using Client.UICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
	public class AdminViewModel : BindableBase
	{
		#region Fields
		#endregion

		#region Properties
		public UIICommand RemoveExpiredRequestsCommand { get; set; }
		#endregion

		#region Constructors
		public AdminViewModel()
		{
			RemoveExpiredRequestsCommand = new UIICommand(OnRemoveExpiredRequests);
		}
		#endregion

		#region Methods
		private void OnRemoveExpiredRequests()
		{
			// TODO: Call Admin service
		}
		#endregion
	}
}
