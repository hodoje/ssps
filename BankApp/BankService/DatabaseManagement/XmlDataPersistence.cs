using System;
using Common.Commanding;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// Xml data persistence class.
	/// </summary>
	class XmlDataPersistence : IDataPersistence
	{
		/// <inheritdoc/>
		public void RemoveItem(long commandId)
		{
			Console.WriteLine($"[XMLDataPersistence] removing command with id: {commandId}");
		}

		/// <inheritdoc/>
		public void SaveItem(BaseCommand item)
		{
			Console.WriteLine($"[XMLDataPersistence] saving command: ({item})");
		}
	}
}
