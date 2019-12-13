using System.ServiceModel;

namespace Common.ServiceInterfaces
{
	[ServiceContract]
	public interface IAdminService
	{
		/// <summary>
		/// Initializes new database.
		/// </summary>
		[OperationContract]
		void CreateNewDatabase();

		/// <summary>
		/// Deletes stale commands.
		/// </summary>
		[OperationContract]
		void DeleteStaleCommands();
	}
}
