using Client.EventArguments;
using Client.UICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
	public class AccessViewModel : BindableBase
	{
		#region Fields
		private string _registrationUsername;
		private string _registrationPassword;
		private string _loginMessage;
		private string _registrationMessage;

		public delegate void LoginEventHandler(object source, LoginEventArgs args);
		public event LoginEventHandler LoggingIn;

		private Visibility _loginErrorMessageVisibility;
		private Visibility _registrationSuccessfulMessageVisibility;
		private Visibility _registrationMessageVisibility;
		#endregion

		#region Properties
		public UIICommand LoginCommand { get; private set; }
		public UIICommand RegisterCommand { get; private set; }
		public string RegistrationUsername
		{
			get { return _registrationUsername; }
			set
			{
				SetField<string>(ref _registrationUsername, value);
				RegisterCommand.RaiseCanExecuteChanged();
			}
		}
		public string RegistrationPassword
		{
			get { return _registrationPassword; }
			set
			{
				SetField<string>(ref _registrationPassword, value);
				RegisterCommand.RaiseCanExecuteChanged();
			}
		}
		public string LoginMessage
		{
			get { return _loginMessage; }
			set
			{
				SetField<string>(ref _loginMessage, value);
			}
		}
		public string RegistrationMessage
		{
			get { return _registrationMessage; }
			set
			{
				SetField<string>(ref _registrationMessage, value);
			}
		}
		public Visibility LoginMessageVisibility
		{
			get { return _loginErrorMessageVisibility; }
			set
			{
				SetField<Visibility>(ref _loginErrorMessageVisibility, value);
			}
		}
		public Visibility RegistrationMessageVisibility
		{
			get { return _registrationMessageVisibility; }
			set
			{
				SetField<Visibility>(ref _registrationMessageVisibility, value);
			}
		}
		public Visibility RegistrationSuccessfulMessageVisibility
		{
			get { return _registrationSuccessfulMessageVisibility; }
			set
			{
				SetField<Visibility>(ref _registrationSuccessfulMessageVisibility, value);
			}
		}
		#endregion

		#region Constructors
		public AccessViewModel()
		{
			LoginCommand = new UIICommand(OnLoggingIn);
			RegisterCommand = new UIICommand(OnRegistering, RegisterCanExecute);

			ResetLoginForm();
			ResetRegistrationForm();
			HideLoginMessage();
			HideRegistrationMessage();
			HideRegistrationSuccessfulMessage();
		}
		#endregion

		#region Methods
		private void OnLoggingIn()
		{
			// TODO: Get user certificate from storage, call bank login service
			//
			//

			bool everythingOk = true;
			LoginMessage = "Set me according to response.";

			if (everythingOk)
			{
				// TODO: Determine if a user is an admin or a regular client
				LoggingIn?.Invoke(this, new LoginEventArgs(UserType.Customer));
			}
			else
			{
				ShowLoginMessage();
				Task.Run(() =>
				{
					Thread.Sleep(2000);
					HideLoginMessage();
				});
			}
		}

		private void OnRegistering()
		{
			// TODO: Call bank registration service, get certificate and store it
			//
			//

			bool everythingOK = true;
			RegistrationMessage = "Set me according to response.";

			if (everythingOK)
			{
				ShowRegistrationSuccessfulMessage();
				ResetRegistrationForm();
			}

			ShowRegistrationMessage();
			Task.Run(() =>
			{
				Thread.Sleep(2000);
				HideRegistrationMessage();
				HideRegistrationSuccessfulMessage();
			});
		}

		private bool RegisterCanExecute()
		{
			return !(String.IsNullOrWhiteSpace(RegistrationUsername) ||
					 String.IsNullOrWhiteSpace(RegistrationPassword));
		}

		private void ResetLoginForm()
		{
			HideLoginMessage();
		}

		private void ResetRegistrationForm()
		{
			RegistrationUsername = "";
			RegistrationPassword = "";
			HideRegistrationMessage();
		}

		private void ShowRegistrationMessage()
		{
			RegistrationMessageVisibility = Visibility.Visible;
		}

		private void HideRegistrationMessage()
		{
			RegistrationMessageVisibility = Visibility.Hidden;
		}

		private void ShowRegistrationSuccessfulMessage()
		{
			RegistrationSuccessfulMessageVisibility = Visibility.Visible;
		}

		private void HideRegistrationSuccessfulMessage()
		{
			RegistrationSuccessfulMessageVisibility = Visibility.Hidden;
		}

		private void ShowLoginMessage()
		{
			LoginMessageVisibility = Visibility.Visible;
		}

		private void HideLoginMessage()
		{
			LoginMessageVisibility = Visibility.Hidden;
		}

		// Subscription to LoggedOut event from MainWindowViewModel
		// which in return subscribed to LoggedOut event from ContentViewModel
		public void OnLoggedOut(object source, EventArgs e)
		{
			ResetLoginForm();
			ResetRegistrationForm();
		}
		#endregion
	}
}
