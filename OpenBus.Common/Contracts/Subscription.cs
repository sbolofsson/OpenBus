using System;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using log4net;
using OpenBus.Common.Security;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    /// <summary>
    /// Describes a subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class Subscription<T> : SystemMessage, ISubscription<T> where T : BusMessage
    {
        [IgnoreDataMember]
        private readonly ILog _logger = LogManager.GetLogger(typeof(Subscription<>));

        // These properties are used on the bus side
        #region Bus side properties

        /// <summary>
        /// Specifies whether subclasses of the type parameter should be included in the subscription.
        /// </summary>
        [DataMember]
        public bool IncludeSubClasses { get; set; }

        /// <summary>
        /// Specifies whether generic subclasses should be included in the subscription.
        /// This only makes sense if the subscribed class adheres to a covariant interface.
        /// </summary>
        [DataMember]
        public bool IncludeGenericSubClasses { get; set; }

        /// <summary>
        /// The queue address of the application making the subscription.
        /// This will be set by the middleware.
        /// </summary>
        [DataMember]
        public string QueueAddress { get; set; }

        #endregion

        // These properties are used on the application side so tell our data contract serializer to ignore them
        #region Application side properties

        /// <summary>
        /// A condition that should hold for the callback handler to be invoked.
        /// </summary>
        [IgnoreDataMember]
        public Func<T, bool> Condition { get; set; }

        /// <summary>
        /// Invokes the callback handler.
        /// If a certificate and/or a condition is specified it will be checked whether they are satisfied.
        /// </summary>
        /// <param name="publish"></param>
        public void InvokeCallbackHandler(IPublish publish)
        {
            // Callback handler ?
            if (CallbackHandler == null)
            {
                _logger.Warn("Could not invoke callback handler because no callback handler is specified.");
                return;
            }

            // No condition and no certificate? The subscriber gets the message
            if (Condition == null && Certificate == null)
            {
                // Invoke asynchronously
                _logger.Debug("Invoking callback handler.");
                CallbackHandler.BeginInvoke(publish.Message as T, null, null);
                return;
            }

            try
            {
                // If we got to here, then the
                // only case we do not invoke callback handler is if the condition exists and does not hold !
                if (Condition != null && !Condition.Invoke(publish.Message as T))
                {
                    _logger.Warn("Could not invoke callback handler because condition was not satisfied.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Could not evalute condition.", ex);
            }

            // Here the condition must be ok, what about certificate?
            if (CertificateHelper.GetCertificate(Certificate) == X509Certificate)
            {
                _logger.Warn("Could not invoke callback handler because no single matching certificate was found.");
                return;
            }

            // If we fail to evaluate the predicate or it holds then we invoke the callback handler
            // It is better to send 'false' data, than data is lost !
            _logger.Debug("Invoking callback handler.");
            CallbackHandler.BeginInvoke(publish.Message as T, null, null);
        }
        
        /// <summary>
        /// A description of the certificate to include when subscribing.
        /// </summary>
        [DataMember]
        public Certificate Certificate { get; set; }
        
        /// <summary>
        /// The actual certificate, will be set by the middleware.
        /// </summary>
        [DataMember]
        public X509Certificate2 X509Certificate { get; set; }
        
        /// <summary>
        /// The callback handler to invoke when relevant messages are published.
        /// </summary>
        [IgnoreDataMember]
        public Action<T> CallbackHandler { get; set; }

        #endregion
    }
}
