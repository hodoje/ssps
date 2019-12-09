using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
	/// <summary>
	/// Represents client proxy for given interface with windows authentication management.
	/// </summary>
	/// <typeparam name="T">Interface which will be used for proxy.</typeparam>
	public class WindowsClientProxy<T> : ChannelFactory<T>, IDisposable
		where T : class
	{
		/// <summary>
		/// Initializes new instance of <see cref="CertificateClientProxy<T>> class."/> 
		/// </summary>
		/// <param name="binding">Service binding.</param>
		/// <param name="address">Service endpoint.</param>
		public WindowsClientProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
		{
			// TODO: set windows security usage
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
