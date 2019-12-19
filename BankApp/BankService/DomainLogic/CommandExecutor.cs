using BankService.DatabaseManagement;
using BankService.DatabaseManagement.Repositories;
using Common;
using Common.Commanding;
using Common.Model;
using System.Threading;
using System.Threading.Tasks;
using System;
using Common.Communication;
using MlkPwgen;
using System.Collections.Generic;
using System.Linq;

namespace BankService.CommandExecutor
{
	public class CommandExecutor : ICommandExecutor
    {
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

		private BankDomainContext bankDbContext;
		private DatabaseManager<User> userDatabaseManager;
		private DatabaseManager<Loan> loanDatabaseManager;
		private DatabaseManager<BankAccount> bankAccDatabaseManager;

		private CommandQueue commandingQueue;

		private IAudit auditService;

        public CommandExecutor(IAudit auditService, CommandQueue commandingQueue)
        {
			this.auditService = auditService;
            this.commandingQueue = commandingQueue;

			SemaphoreSlim dbSynchronazation = new SemaphoreSlim(1);
			bankDbContext = ServiceLocator.GetObject<BankDomainContext>();
			userDatabaseManager = new DatabaseManager<User>(new Repository<User>(bankDbContext, dbSynchronazation));
			loanDatabaseManager = new DatabaseManager<Loan>(new Repository<Loan>(bankDbContext, dbSynchronazation));
			bankAccDatabaseManager = new DatabaseManager<BankAccount>(new Repository<BankAccount>(bankDbContext, dbSynchronazation));
		}

        public void Start()
        {
            Task listenerThread = new Task(WorkerThread);
            listenerThread.Start();
        }

        public void Stop()
        {
            cancellationToken.Cancel();
        }

        private void WorkerThread()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BaseCommand command = commandingQueue.Dequeue();

                if (command == null)
                {
                    continue;
                }

				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				ExecuteCommand(command);
            }
        }

		public void CreateDatabase()
		{
			bankDbContext.Database.Create();
			bankDbContext.Database.Connection.Open();

			auditService.Log(logMessage: "New BankDomainDB database created!", eventLogEntryType: System.Diagnostics.EventLogEntryType.Warning);
		}

		private void ExecuteCommand(BaseCommand command)
		{
			Type commandType = command.GetType();

			if (commandType == typeof(TransactionCommand))
			{
				Transaction(command);
			}
			else if (commandType == typeof(RegistrationCommand))
			{
				RequestNewCard(command);
			}
			else if (commandType == typeof(RequestLoanCommand))
			{
				RequestLoan(command);
			}
		}

		private void Transaction(BaseCommand command)
		{
			TransactionCommand transactionCommand = command as TransactionCommand;

			BankAccount bankAccount = bankAccDatabaseManager.Get(transactionCommand.BankAccountId);
			if (bankAccount == null)
			{
				auditService.Log(transactionCommand.Username, "Requested transaction on bank account which is non existent!", System.Diagnostics.EventLogEntryType.Warning);
				return;
			}

			bankAccount.Amount += transactionCommand.TransactionType == TransactionType.Deposit ? transactionCommand.Amount : -transactionCommand.Amount;
			bankAccDatabaseManager.Update(bankAccount);

			auditService.Log("CommandExecutor", $"Transaction type {transactionCommand.TransactionType.ToString()} with {transactionCommand.Amount} successfully executed.");
		}

		private void RequestNewCard(BaseCommand command)
		{
			RegistrationCommand registrationCommand = command as RegistrationCommand;

			User user = userDatabaseManager.FindEntity(x => x.Username == registrationCommand.Username);
			if (user == null)
			{
				user.Username = command.Username;
				userDatabaseManager.AddEntity(user);
			}

			long accountId = long.Parse(PasswordGenerator.Generate(10, new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
			BankAccount newAccount = new BankAccount(accountId);

			bankAccDatabaseManager.AddEntity(newAccount);
			newAccount.User = user;
			bankAccDatabaseManager.Update(newAccount);

			auditService.Log("CommandExecutor", $"New card successfully created for user: {user.Username}.");

		}

		private void RequestLoan(BaseCommand command)
		{
			RequestLoanCommand loanCommand = command as RequestLoanCommand;

			User user = userDatabaseManager.FindEntity(x => x.Username == loanCommand.Username);
			if (user == null)
			{
				user.Username = command.Username;
				userDatabaseManager.AddEntity(user);
			}

			Loan newLoan = new Loan() { User = user };
			loanDatabaseManager.AddEntity(newLoan);
		}

		public List<BankAccount> GetUsersAccount(string username)
		{
			User user = userDatabaseManager.FindEntity(x => x.Username == username);
			if (user == null)
			{
				auditService.Log("CommandExecutor", $"User with {username} does not exist!", System.Diagnostics.EventLogEntryType.Warning);
				return new List<BankAccount>(0);
			}

			return bankAccDatabaseManager.Find(x => x.UserId == user.ID).ToList();
		}
	}
}
