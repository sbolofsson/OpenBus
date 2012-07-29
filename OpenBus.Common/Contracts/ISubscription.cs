using System.Security.Cryptography.X509Certificates;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    // Covariant interface allows us to convert between generic derived classes and base class
    // Source: http://msdn.microsoft.com/en-us/library/dd799517.aspx
    public interface ISubscription<out T> where T : BusMessage
    {
        // Bus side properties
        /// <summary>
        /// Specifies whether generic subclasses should be included in the subscription.
        /// This only makes sense if the subscribed class adheres to a covariant interface.
        /// </summary>
        bool IncludeGenericSubClasses { get; set; }

        /// <summary>
        /// Specifies whether subclasses of the type parameter should be included in the subscription.
        /// </summary>
        bool IncludeSubClasses { get; set; }

        /// <summary>
        /// The queue address of the application making the subscription.
        /// This will be set by the middleware.
        /// </summary>
        string QueueAddress { get; set; }

        // Application side properties

        /// <summary>
        /// A description of the certificate to include when subscribing.
        /// </summary>
        Certificate Certificate { get; set; }

        /// <summary>
        /// The actual certificate, will be set by the middleware.
        /// </summary>
        X509Certificate2 X509Certificate { get; set; }
        
        /// <summary>
        /// Invokes the callback handler.
        /// If a certificate and/or a condition is specified it will be checked whether they are satisfied.
        /// </summary>
        /// <param name="publish"></param>
        void InvokeCallbackHandler(IPublish publish);

    }
}
