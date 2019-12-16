using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankService.CommandExecutor
{
    public interface ICommandExecutor
    {
        void Start();
        void Stop();
    }
}
