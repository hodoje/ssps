using BankService.DatabaseManagement;
using BankService.DatabaseManagement.Repositories;
using Common;
using Common.Commanding;
using Common.Model;
using System.Threading;
using System.Threading.Tasks;
using System;
using Common.Communication;

namespace BankService.CommandExecutor
{
	public class CommandExecutor : ICommandExecutor
    {
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
		private BankDomainContext bankDbContext;

		private DatabaseManager<User> userDatabaseManager;
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

                // execute command
            }
        }

		public void CreateDatabase()
		{
			bankDbContext.Database.Create();
			bankDbContext.Database.Connection.Open();

			auditService.Log(logMessage: "New BankDomainDB database created!", eventLogEntryType: System.Diagnostics.EventLogEntryType.Warning);
		}
	}
}
