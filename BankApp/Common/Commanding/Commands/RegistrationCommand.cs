using System.Runtime.Serialization;

namespace Common.Commanding
{
	/// <summary>
	/// Command used by users to register to banking system.
	/// </summary>
	[DataContract]
	public class RegistrationCommand : BaseCommand
	{
		/// <summary>
		/// Initializes new instance of <see cref="RegistrationCommand"/> class. 
		/// </summary>
		/// <param name="commandId">Unique commanding ID.</param>
		/// <param name="username">New users username.</param>
		/// <param name="password">New users password.</param>
		public RegistrationCommand(long commandId, string username, string password) : base(commandId)
		{
			Username = username;
			Password = password;
		}

		/// <summary>
		/// Username.
		/// </summary>
		[DataMember]
		public string Username { get; private set; }

		/// <summary>
		/// Password.
		/// </summary>
		[DataMember]
		public string Password { get; private set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			RegistrationCommand registraionCommand = obj as RegistrationCommand;
			if (registraionCommand == null)
			{
				return false;
			}

			return base.Equals(obj) && Username == registraionCommand.Username && Password == registraionCommand.Password;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
