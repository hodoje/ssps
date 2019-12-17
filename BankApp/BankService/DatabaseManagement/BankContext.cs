using Common.Commanding;
using Common.Model;
using System.Data.Entity;

namespace BankService.DatabaseManagement
{
	public class BankContext : DbContext
	{
		public BankContext() : base("DefaultConnection")
		{
			Database.SetInitializer<BankContext>(null);
		}
		public BankContext(string stringConnection) : base(stringConnection)
		{
			Database.SetInitializer<BankContext>(null);
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<NotificationInformation>()
			   .Ignore(b => b.UserCallback);
		}

		public DbSet<BaseCommand> Commands { get; set; }
		public DbSet<NotificationInformation> Notifications { get; set; }
		public DbSet<CommandNotification> ReadyNotifications { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<BankAccount> BankAccounts { get; set; }
	}
}
