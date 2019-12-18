namespace Common.Model
{
	public class BankAccount : IdentifiedObject
	{
		public BankAccount() : base() { }
		public BankAccount(string accountNumber)
		{

		}

		public string AccountNumber { get; set; }
		public double Amount { get; set; }
		public int UserId { get; set; }
		public User User { get; set; }
	}
}
