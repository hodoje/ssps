using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
	public class BankAccount
	{
		public int Id { get; set; }
		public string AccountNumber { get; set; }
		public double Amount { get; set; }
		public int UserId { get; set; }
		public User User { get; set; }
	}
}
