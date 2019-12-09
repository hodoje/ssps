using Client.Model;
using Client.UICommands;
using Common.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
	public class UserViewModel : BindableBase
	{
		#region Fields
		private double? _loanAmount;
		private int? _loanDuration;
		private double? _transactionAmount;
		private TransactionType _selectedTransactionType;
		#endregion

		#region Properties
		public double? LoanAmount
		{
			get { return _loanAmount; }
			set
			{
				SetField(ref _loanAmount, value);
				ApplyForCreditCommand.RaiseCanExecuteChanged();
			}
		}

		public int? LoanDuration
		{
			get { return _loanDuration; }
			set
			{
				SetField(ref _loanDuration, value);
				ApplyForCreditCommand.RaiseCanExecuteChanged();
			}
		}

		public double? TransactionAmount
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
		#endregion

		#region Constructors
		public UserViewModel()
		{
			TransactionTypes = new ObservableCollection<TransactionType>(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>());
			Notifications = new ObservableCollection<Notification>();

			ApplyForCreditCommand = new UIICommand(OnApplyForCredit, CanApplyForCredit);
			ExecuteTransactionCommand = new UIICommand(OnExecuteTransaction, CanExecuteTransaction);
		}
		#endregion

		#region Methods
		#region CommandMethods
		private void OnExecuteTransaction()
		{
			// TODO: Call user service
			switch (SelectedTransactionType)
			{
				case TransactionType.Payment:
					// TODO: Call user service for payment
					break;
				case TransactionType.Withdrawal:
					// TODO: Call user service for withdrawal
					break;
			}
		}

		private bool CanExecuteTransaction()
		{
			return (TransactionAmount != null && TransactionAmount > 0);
		}

		private void OnApplyForCredit()
		{
			// TODO: Call user service for loan raising
		}

		private bool CanApplyForCredit()
		{
			return (LoanAmount != null && LoanAmount > 0 && LoanDuration != null && LoanDuration > 0);
		}
		#endregion

		private void StartListeningForNotifications()
		{
			// TODO: Open service and start listening
		}

		private void ProcessNotifications()
		{
			// TODO: Add notifications to the Notifications collection
		}
		#endregion
	}
}
