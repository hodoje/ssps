﻿using Common.Commanding;

namespace Common.ServiceInterfaces
{
	/// <summary>
	/// Exposes sector methods.
	/// </summary>
	public interface ISectorService
	{
		/// <summary>
		/// Sends request to sector which will be processed asynchronous.
		/// </summary>
		/// <param name="command">Command to be executed on sector.</param>
		/// <param name="integrityCheck">Integrity check bytes.</param>
		void SendRequest(BaseCommand command, byte[] integrityCheck);
	}
}
