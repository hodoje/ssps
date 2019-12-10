using System;
using System.IO;
using System.Collections.Generic;
using Common.Model;

namespace BankService.DatabaseManagement
{
	/// <summary>
	/// XML data persistence class.
	/// </summary>
	public class XmlDataPersistence<T> : IDataPersistence<IdentifiedObject>, IDisposable
		where T : IdentifiedObject
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
		public static IDataPersistence<T> CreatePersister(string path)
		{
			XmlDataPersistence<T> parser = null;

			if (File.Exists(path))
			{
				parser = new XmlDataPersistence<T>(path);
			}

			return (IDataPersistence<T>)parser;
		}

		/// <summary>
		/// Creates
		/// </summary>
		/// <param name="path">Path to XML file.</param>
		public static void CreateXmlFile(string path)
		{
			File.Create(path);
		}

		public void Dispose()
		{
			// close XML file handler
		}

		/// <inheritdoc/>
		public List<IdentifiedObject> ReadAllEntities()
		{
			// todo READ FROM XML
			Console.WriteLine($"[XMLDataPersistence<{typeof(T)}>] reading all entities.");
			return new List<IdentifiedObject>();
		}

		/// <inheritdoc/>
		public void RemoveEntity(long commandId)
		{
			Console.WriteLine($"[XMLDataPersistence<{typeof(T)}>] removing entity with id: {commandId}.");
		}

		/// <inheritdoc/>
		public void AddEntity(IdentifiedObject item)
		{
			Console.WriteLine($"[XMLDataPersistence<{typeof(T)}>] saving entity: ({item}).");
		}
	}
}
