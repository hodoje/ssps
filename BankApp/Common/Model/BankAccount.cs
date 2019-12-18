namespace Common.Model
{
	public class BankAccount : IdentifiedObject
	{
		public BankAccount() : base() { }
		public BankAccount(long accountNumber) : base(accountNumber)
		{

		}

		public double Amount { get; set; }
		public long UserId { get; set; }
		public User User { get; set; }
	}
}
