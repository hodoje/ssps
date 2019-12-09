using Client.EventArguments;
using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Client
{
	public class MainWindowViewModel : BindableBase
	{
		#region Fields
		private IUnityContainer _container;
		private AccessViewModel _accessViewModel;
		private ContentViewModel _contentViewModel;
		private BindableBase _currentViewModel;

		public delegate void LoggingOutEventHandler(object source, EventArgs args);
		public event LoggingOutEventHandler LoggedOut;
		#endregion

		#region Properties
		public BindableBase CurrentViewModel
		{
			get { return _currentViewModel; }
			set { SetField(ref _currentViewModel, value); }
		}
		#endregion

		#region Constructors
		public MainWindowViewModel(IUnityContainer container)
		{
			_container = container;

			_accessViewModel = _container.Resolve<AccessViewModel>();
			_accessViewModel.LoggingIn += OnLoggingIn;

			_contentViewModel = _container.Resolve<ContentViewModel>();
			_contentViewModel.LoggedOut += OnLoggedOut;

			LoggedOut += _accessViewModel.OnLoggedOut;

			CurrentViewModel = _accessViewModel;
		}
		#endregion

		#region Methods
		// Subscription to Logging in event from AccessViewModel
		private void OnLoggingIn(object source, LoginEventArgs e)
		{
			CurrentViewModel = _contentViewModel;
			switch (e.UserType)
			{
				case UserType.Admin:
					_contentViewModel.DisplayAdminView();
					break;
				case UserType.Customer:
					_contentViewModel.DisplayCustomerView();
					break;
			}			
		}

		// Subscription to LoggingOut event from ContentViewModel
		private void OnLoggedOut(object source, EventArgs e)
		{
			LoggedOut?.Invoke(this, EventArgs.Empty);
			CurrentViewModel = _accessViewModel;
		}
		#endregion
	}
}
