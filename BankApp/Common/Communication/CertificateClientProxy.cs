using System;
using System.ServiceModel;

namespace Common.Communication
{
	/// <summary>
	/// Represents client proxy for given interface with certificate management.
	/// </summary>
	/// <typeparam name="T">Interface which will be used for proxy.</typeparam>
	public class CertificateClientProxy<T> : ChannelFactory<T>, IDisposable 
		where T : new()
	{
		/// <summary>
		/// Initializes new instance of <see cref="CertificateClientProxy<T>> class."/> 
		/// </summary>
		/// <param name="binding">Service binding.</param>
		/// <param name="address">Service endpoint.</param>
		public CertificateClientProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
		{
			// todo add certificate

			Proxy = this.CreateChannel();
		}

		/// <summary>
		/// Proxy used for communication with service.
		/// </summary>
		public T Proxy { get; private set; }

		/// <inheritdoc/>
		public void Dispose()
		{
			if (Proxy != null)
			{
				Proxy = default(T);
			}

			this.Close();
		}
	}
}
