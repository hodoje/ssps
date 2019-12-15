using Client.Model;
using Client.UICommands;
using Common.CertificateManagement;
using Common.Commanding;
using Common.Communication;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.ViewModels
{
	public class UserViewModel : BindableBase
	{
		#region Fields
		private double _loanAmount = 100;
		private int _loanDuration = 1;
		private double _transactionAmount;
		private TransactionType _selectedTransactionType;
		private CertificateClientProxy<IUserService> _userServiceProxy;
		private CertificateClientProxy<IAdminService> _adminServiceProxy;
		public BankServiceCallbackObject _userServiceCallbackObject;
		#endregion

		#region Properties
		public double LoanAmount
		{
			get { return _loanAmount; }
			set
			{
				SetField(ref _loanAmount, value);
				ApplyForCreditCommand.RaiseCanExecuteChanged();
			}
		}

		public int LoanDuration
		{
			get { return _loanDuration; }
			set
			{
				SetField(ref _loanDuration, value);
				ApplyForCreditCommand.RaiseCanExecuteChanged();
			}
		}

		public double TransactionAmount
		{
			get { return _transactionAmount; }
			set
			{
				SetField(ref _transactionAmount, value);
				ExecuteTransactionCommand.RaiseCanExecuteChanged();
			}
		}

		public TransactionType SelectedTransactionType
		{
			get { return _selectedTransactionType; }
			set
			{
				SetField(ref _selectedTransactionType, value);
				ExecuteTransactionCommand.RaiseCanExecuteChanged();
			}
		}
		public ObservableCollection<TransactionType> TransactionTypes { get; set; }
		public ObservableCollection<Notification> Notifications { get; set; }
		public UIICommand ApplyForCreditCommand { get; set; }
		public UIICommand ExecuteTransactionCommand { get; set; }
		public UIICommand CreateNewDatabaseCommand { get; set; }
		public UIICommand RemoveExpiredRequestsCommand { get; set; }
		#endregion

		#region Constructors
		public UserViewModel()
		{
			TransactionTypes = new ObservableCollection<TransactionType>(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>());
			Notifications = new ObservableCollection<Notification>();

			ApplyForCreditCommand = new UIICommand(OnApplyForCredit, CanApplyForCredit);
			ExecuteTransactionCommand = new UIICommand(OnExecuteTransaction, CanExecuteTransaction);
			CreateNewDatabaseCommand = new UIICommand(OnCreateNewDatabase);
			RemoveExpiredRequestsCommand = new UIICommand(OnRemoveExpiredRequests);

			_userServiceCallbackObject = new BankServiceCallbackObject(HandleNotifications);

			//string username = StringFormatter.ParseName(WindowsIdentity.GetCurrent().Name);
			// TEST
			string username = "user1";
			X509Certificate2 certificate = GetCertificateFromStorage(username);

			_userServiceProxy = new CertificateClientProxy<IUserService>(_userServiceCallbackObject, ClientConfig.BankServiceAddress, ClientConfig.UserServiceEndpoint, certificate);
			_adminServiceProxy = new CertificateClientProxy<IAdminService>(_userServiceCallbackObject, ClientConfig.BankServiceAddress, ClientConfig.AdminServiceEndpoint, certificate);
		}
		#endregion

		#region Methods
		#region CommandMethods
		private void OnExecuteTransaction()
		{
			try
			{
				// TODO: Call user service
				switch (SelectedTransactionType)
				{
					case TransactionType.Payment:
						_userServiceProxy.Proxy.Deposit(TransactionAmount);
						break;
					case TransactionType.Withdrawal:
						_userServiceProxy.Proxy.Withdraw(TransactionAmount);
						break;
				}
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification("Service has denied access since you have no permission for action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
			}
		}

		private bool CanExecuteTransaction()
		{
			return TransactionAmount > 0;
		}

		private void OnApplyForCredit()
		{
			try
			{
				_userServiceProxy.Proxy.Register();
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification("Service has denied access since you have no permission for action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
			}
		}

		private bool CanApplyForCredit()
		{
			return LoanAmount > 0 && LoanDuration > 0;
		}
		#endregion

		private void StartListeningForNotifications()
		{
			// TODO: Open service and start listening
		}

		private void HandleNotifications(CommandNotification commandNotification)
		{
			Notification notification = new Notification(commandNotification.Information, commandNotification.CommandStatus);
			Notifications.Add(notification);
		}

		private void OnRemoveExpiredRequests()
		{
			try
			{
				_adminServiceProxy.Proxy.DeleteStaleCommands();
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification("Service has denied access since you have no permission for action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
			}
		}

		private void OnCreateNewDatabase()
		{
			try
			{
				_adminServiceProxy.Proxy.CreateNewDatabase();
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification("Service has denied access since you have no permission for action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
			}
		}

		private X509Certificate2 GetCertificateFromStorage(string cltCertCN)
		{
			X509Certificate2 certificate;
			if (CertificateStorageReader.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN, out certificate))
			{
				return certificate;
			}
			else
			{
				throw new ArgumentException("Service will not be initializes since it has no valid certificate!");
			}
		}

		#endregion
	}
}
