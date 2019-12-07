using Common.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Interface which exposes methods of a database management unit.
	/// </summary>
	public interface IDatabaseManager
	{
		/// <summary>
		/// Used for concurrent reading all commands which are currently in database.
		/// </summary>
		/// <returns>Commands which are in database.</returns>
		List<BaseCommand> ReadAllCommands();

		/// <summary>
		/// Used for concurrent saving given command to the database.
		/// </summary>
		/// <param name="baseCommand">Command to be saved to database.</param>
		void SaveCommand(BaseCommand command);

		/// <summary>
		/// Used to concurrent removing command by id from database.
		/// </summary>
		/// <param name="commandId">Command id to remove from database.</param>
		void RemoveCommand(long commandId);
	}
}
