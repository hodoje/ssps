using Common.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankService.CommandExecutor
{
    public class CommandExecutor : ICommandExecutor
    {
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        
        // todo add DataManager<User> + DataManager<Account> from ServiceLocator

        private CommandQueue commandingQueue;

        public CommandExecutor(CommandQueue commandingQueue)
        {
            this.commandingQueue = commandingQueue;
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
    }
}
