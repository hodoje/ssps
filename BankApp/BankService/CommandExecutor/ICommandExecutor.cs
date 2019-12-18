using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankService.CommandExecutor
{
	/// <summary>
	/// Interface exposes methods for unit which is responsible for executing domain logic.
	/// </summary>
    public interface ICommandExecutor
    {
        void Start();
        void Stop();
		void CreateDatabase();
    }
}
