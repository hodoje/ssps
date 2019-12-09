using System;
using Common.Commanding;
using System.IO;
using System.Collections.Generic;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// XML data persistence class.
	/// </summary>
	public class XmlDataPersistence : IDataPersistence
	{
		private string xmlPath;
		/// <summary>
		/// Initializes new instance of <see cref="XmlDataPersistence"/> class.
		/// </summary>
		/// <param name="xmlPath"></param>
		public XmlDataPersistence(string xmlPath)
		{
			this.xmlPath = xmlPath;
		}

		/// <summary>
		/// Creates new instance of XML parser.
		/// </summary>
		/// <param name="path">Path to XML file.</param>
		/// <returns>Returns new instance of <see cref="XmlDataPersistence"/> if XML file exists on the path, otherwise <b>null</b>.</returns>
		public static XmlDataPersistence CreateParser(string path)
		{
			XmlDataPersistence parser = null;

			if (File.Exists(path))
			{
				parser = new XmlDataPersistence(path);
			}

			return parser;
		}

		/// <summary>
		/// Creates
		/// </summary>
		/// <param name="path">Path to XML file.</param>
		public static void CreateXmlFile(string path)
		{
			File.Create(path);
		}

		/// <inheritdoc/>
		public List<BaseCommand> ReadAllCommands()
		{
			// todo READ FROM XML
			Console.WriteLine($"[XMLDataPersistence] reading all commands.");
			return new List<BaseCommand>();
		}

		/// <inheritdoc/>
		public void RemoveItem(long commandId)
		{
			Console.WriteLine($"[XMLDataPersistence] removing command with id: {commandId}.");
		}

		/// <inheritdoc/>
		public void SaveItem(BaseCommand item)
		{
			Console.WriteLine($"[XMLDataPersistence] saving command: ({item}).");
		}
	}
}
