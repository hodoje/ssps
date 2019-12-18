using Common.Model;
using System.Data.Entity;

namespace BankService.DatabaseManagement
{
	public class BankDomainContext : DbContext
	{
		public BankDomainContext() : base("DefaultDomainString")
		{
			Database.SetInitializer<BankCommandingContext>(null);
		}
		public BankDomainContext(string stringConnection) : base(stringConnection)
		{
			Database.SetInitializer<BankCommandingContext>(null);
		}

		public DbSet<User> Users { get; set; }
		public DbSet<BankAccount> BankAccounts { get; set; }
	}
}
