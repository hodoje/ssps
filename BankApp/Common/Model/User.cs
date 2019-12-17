using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public ICollection<BankAccount> Accounts { get; set; }
	}
}
