using System.Security.Cryptography.X509Certificates;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    // Parametrized interface
    public interface IPublish
    {
        // Application side properties

        /// <summary>
        /// A description of the certificate to include when publishing.
        /// </summary>
        Certificate Certificate { get; set; }

        /// <summary>
        /// The actual certificate, will be set by the middleware.
        /// </summary>
        X509Certificate2 X509Certificate { get; set; }

        /// <summary>
        /// The message to be published.
        /// </summary>
        BusMessage Message { get; set; }
    }
}
