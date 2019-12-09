using Client.UICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Client.ViewModels
{
	public class ContentViewModel : BindableBase
	{
		#region Fields
		private IUnityContainer _container;
		private BindableBase _currentContentViewModel;
		private UserViewModel _userViewModel;
		private AdminViewModel _adminViewModel;
		public delegate void LogoutEventHandler(object sender, EventArgs args);
		public event LogoutEventHandler LoggedOut;
		#endregion

		#region Properties
		public BindableBase CurrentContentViewModel
		{
			get { return _currentContentViewModel; }
			set { SetField(ref _currentContentViewModel, value); }
		}

		public UIICommand LogoutCommand { get; private set; }
		#endregion

		#region Constructors
		public ContentViewModel(IUnityContainer container)
		{
			_container = container;

			_userViewModel = _container.Resolve<UserViewModel>();
			_adminViewModel = _container.Resolve<AdminViewModel>();

			LogoutCommand = new UIICommand(OnLoggingOut);
		}
		#endregion

		#region Methods
		public void DisplayCustomerView()
		{
			CurrentContentViewModel = _userViewModel;
		}

		public void DisplayAdminView()
		{			
			CurrentContentViewModel = _adminViewModel;
		}

		private void OnLoggingOut()
		{
			LoggedOut?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
