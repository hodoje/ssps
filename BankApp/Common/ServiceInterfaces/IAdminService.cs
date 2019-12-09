namespace Common.ServiceInterfaces
{
	public interface IAdminService
	{
		/// <summary>
		/// Initializes new database.
		/// </summary>
		void CreateNewDatabase();

		/// <summary>
		/// Deletes stale commands.
		/// </summary>
		void DeleteStaleCommands();
	}
}
