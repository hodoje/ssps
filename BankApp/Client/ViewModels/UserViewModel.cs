﻿using Client.Model;
using Client.UICommands;
using Common.CertificateManagement;
using Common.Commanding;
using Common.Communication;
using Common.Model;
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
using System.Windows;

namespace Client.ViewModels
{
	public class UserViewModel : BindableBase
	{
		#region Fields
		private double _loanAmount = 0;
		private int _loanDuration = 0;
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
				RequestLoanCommand.RaiseCanExecuteChanged();
			}
		}

		public int LoanDuration
		{
			get { return _loanDuration; }
			set
			{
				SetField(ref _loanDuration, value);
				RequestLoanCommand.RaiseCanExecuteChanged();
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
		public ObservableCollection<BankAccount> BankAccounts { get; set; }
		public UIICommand RequestLoanCommand { get; set; }
		public UIICommand ExecuteTransactionCommand { get; set; }
		public UIICommand CreateNewDatabaseCommand { get; set; }
		public UIICommand RemoveExpiredRequestsCommand { get; set; }
		public UIICommand CreateNewBankAccountCommand { get; set; }
		#endregion

		#region Constructors
		public UserViewModel()
		{
			TransactionTypes = new ObservableCollection<TransactionType>(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>());
			Notifications = new ObservableCollection<Notification>();
			BankAccounts = new ObservableCollection<BankAccount>();

			RequestLoanCommand = new UIICommand(OnRequestLoan, CanRequestLoan);
			ExecuteTransactionCommand = new UIICommand(OnExecuteTransaction, CanExecuteTransaction);
			CreateNewDatabaseCommand = new UIICommand(OnCreateNewDatabase);
			RemoveExpiredRequestsCommand = new UIICommand(OnRemoveExpiredRequests);
			CreateNewBankAccountCommand = new UIICommand(OnCreateNewBankAccount);

			_userServiceCallbackObject = new BankServiceCallbackObject(HandleNotifications);

			//string username = StringFormatter.ParseName(WindowsIdentity.GetCurrent().Name);
			//TEST
			string username = "user1";
			X509Certificate2 certificate;
			certificate = GetCertificateFromStorage(username);
			if (certificate == null)
			{
				Environment.Exit(0);
				return;
			}
			
			_userServiceProxy = new CertificateClientProxy<IUserService>(_userServiceCallbackObject, ClientConfig.BankServiceAddress, ClientConfig.UserServiceEndpoint, certificate);
			_adminServiceProxy = new CertificateClientProxy<IAdminService>(_userServiceCallbackObject, ClientConfig.BankServiceAddress, ClientConfig.AdminServiceEndpoint, certificate);
            if(StringFormatter.GetAttributeFromSubjetName(certificate.SubjectName.Name, "OU") == "users")
            {
                AskServiceForNotifications();
				GetAllBankAccounts();
			}
		}
		#endregion

		#region Methods
		#region CommandMethods
		private void OnExecuteTransaction()
		{
			try
			{
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
				Notifications.Add(new Notification($"{SelectedTransactionType} transaction action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if(_userServiceProxy == null)
				{
					Notifications.Add(new Notification("User service unavailable.", CommandNotificationStatus.Rejected));
				}
				else
				{
					Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
				}
			}
		}

		private bool CanExecuteTransaction()
		{
			return TransactionAmount > 0;
		}

		private void OnRequestLoan()
		{
			try
			{
				_userServiceProxy.Proxy.RequestLoan(LoanAmount, LoanDuration);
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Request loan action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_userServiceProxy == null)
				{
					Notifications.Add(new Notification("User service unavailable.", CommandNotificationStatus.Rejected));
				}
				else
				{
					Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
				}
			}
		}

		private bool CanRequestLoan()
		{
			return LoanAmount > 0 && LoanDuration > 0;
		}

		private void OnCreateNewBankAccount()
		{
			try
			{
				_userServiceProxy.Proxy.Register();
				GetAllBankAccounts();
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Create new bank account action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_userServiceProxy == null)
				{
					Notifications.Add(new Notification("User service unavailable.", CommandNotificationStatus.Rejected));
				}
				else
				{
					Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
				}
			}
		}

		private void OnRemoveExpiredRequests()
		{
			try
			{
				CommandNotification cn = _adminServiceProxy.Proxy.DeleteStaleCommands();
				Notifications.Add(new Notification(cn.Information, cn.CommandStatus));
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Remove expired requests action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_adminServiceProxy == null)
				{
					Notifications.Add(new Notification("Admin service unavailable.", CommandNotificationStatus.Rejected));
				}
				else
				{
					Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
				}
			}
		}

		private void OnCreateNewDatabase()
		{
			try
			{
				// TODO: get notification, cuz admin operations are synchronous
				CommandNotification cn = _adminServiceProxy.Proxy.CreateNewDatabase();
				Notifications.Add(new Notification(cn.Information, cn.CommandStatus));
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Create new database action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_adminServiceProxy == null)
				{
					Notifications.Add(new Notification("Admin service unavailable.", CommandNotificationStatus.Rejected));
				}
				else
				{
					Notifications.Add(new Notification(e.Message, CommandNotificationStatus.Rejected));
				}
			}
		}
		#endregion

		private void GetAllBankAccounts()
		{
			try
			{
				List<BankAccount> bankAccounts = _userServiceProxy.Proxy.GetMyBankAccounts();

				if(BankAccounts.Count > 0)
				{
					BankAccounts.Clear();
				}

				foreach(var ba in bankAccounts)
				{
					BankAccounts.Add(ba);
				}
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Get my bank accounts action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_userServiceProxy == null)
				{
					Notifications.Add(new Notification("Unable to get your bank accounts since user service unavailable.", CommandNotificationStatus.Rejected));
				}
			}
		}

		private void AskServiceForNotifications()
		{
			try
			{
				List<CommandNotification> notifications = _userServiceProxy.Proxy.GetPendingNotifications();
				foreach (CommandNotification notification in notifications)
				{
					Notifications.Add(new Notification(notification.Information, notification.CommandStatus));
				}
			}
			catch (SecurityAccessDeniedException securityAccess)
			{
				Notifications.Add(new Notification($"Get my notification action denied. You have no permission for this action.", CommandNotificationStatus.Rejected));
			}
			catch (Exception e)
			{
				if (_userServiceProxy == null)
				{
					Notifications.Add(new Notification("User service unavailable.", CommandNotificationStatus.Rejected));
				}
			}
		}

		private void HandleNotifications(CommandNotification commandNotification)
		{
			Notification notification = new Notification(commandNotification.Information, commandNotification.CommandStatus);
			Notifications.Add(notification);
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
				//throw new ArgumentException("Service will not be initializes since it has no valid certificate!");
				MessageBox.Show("Unable to connect to Bank since you don't have a valid certificate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		#endregion
	}
}
