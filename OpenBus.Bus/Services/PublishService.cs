using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Activation;
using OpenBus.Common.Contracts;
using log4net;
using System.ServiceModel;
using OpenBus.Common.Security;
using OpenBus.Common.Serialization;
using OpenBus.Common.Services;
using OpenBus.Messages;

namespace OpenBus.Bus.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple),
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PublishService : IPublisher<BusMessage>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(PublishService));

        private static int i = 0;

        /// <summary>
        /// Publish a message on the bus
        /// </summary>
        /// <param name="publish"></param>
        public void Publish(IPublish publish)
        {
            if (publish == null || publish.Message == null)
            {
                _logger.Error("Cannot publish a null message.");
                return;
            }

            List<ISubscription<BusMessage>> subscribers = Bus.GetSubscribers(publish.Message.GetType());

            if (subscribers == null || subscribers.Count == 0)
            {
                _logger.Error(String.Format("No subscribers found for type '{0}'. Could not distribute message.", publish.Message.GetType().FullName));
                return;
            }

            _logger.Info(String.Format("Found {0} subscribers.", subscribers.Count));
            
            foreach (ISubscription<BusMessage> subscriber in subscribers)
            {
                try
                {
                    _logger.Info(String.Format("{0} putting message on subscribers queue '{1}'.", DateTime.Now, subscriber));
                    publish.Message.PublishServiceTime = DateTime.Now;
                    SendMessage(subscriber.QueueAddress, publish);
                }
                catch (Exception ex)
                {
                    _logger.Error(String.Format("Failed putting message on subscribers queue '{0}'.", subscriber), ex);
                }
            }
        }

        /// <summary>
        /// Send the actual message to the subscriber.
        /// </summary>
        /// <param name="queueAddress">The subscriber's address.</param>
        /// <param name="busMessage">The message to send.</param>
        private void SendMessage(string queueAddress, IPublish busMessage)
        {
            if (String.IsNullOrEmpty(queueAddress) || busMessage == null)
            {
                _logger.Error("Could not send message to subscriber because he was null or the message was null.");
                return;
            }

            try
            {
                ChannelFactory<ISubscriber<BusMessage>> channelFactory = GetChannelFactory(queueAddress);
                ISubscriber<BusMessage> proxy = channelFactory.CreateChannel();
                proxy.OnMessagePublished(busMessage);
                channelFactory.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        /// <summary>
        /// Dynamically gets a channel factory based on the client
        /// </summary>
        /// <param name="queueAddress"></param>
        /// <returns></returns>
        private ChannelFactory<ISubscriber<BusMessage>> GetChannelFactory(string queueAddress)
        {
            if (String.IsNullOrEmpty(queueAddress))
            {
                _logger.Error("Could not get channel factory because subscriber was null or empty.");
                return null;
            }

            // For now we just assume that the last part of the queue name equals the client name
            var index = queueAddress.LastIndexOf(@"/");

            if (index < 0)
            {
                _logger.Error("Could not get subscribers queue name. It should be the last part of the subscriber string.");
                return null;
            }

            string clientName = queueAddress.Substring(index + 1);

            // Set the endpoint identity dns to the client's name
            EndpointIdentity endpointIdentity = EndpointIdentity.CreateDnsIdentity(clientName);
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(queueAddress), endpointIdentity);
            NetMsmqBinding netMsmqBinding = new NetMsmqBinding("SecureBinding");
            ChannelFactory<ISubscriber<BusMessage>> channelFactory = new ChannelFactory<ISubscriber<BusMessage>>(netMsmqBinding, endpointAddress);
            
            // Add the client's certificate
            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, clientName);

                // Add the bus' certificate
                channelFactory.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, "OpenBus");
            }

            return channelFactory;
        }
    }
}
