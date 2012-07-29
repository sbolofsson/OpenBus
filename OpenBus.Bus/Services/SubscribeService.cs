using System;
using System.ServiceModel.Activation;
using OpenBus.Common.Contracts;
using log4net;
using System.ServiceModel;
using OpenBus.Common.Serialization;
using OpenBus.Common.Services;
using OpenBus.Messages;

namespace OpenBus.Bus.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple),
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SubscribeService : ISubscriber<BusMessage>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SubscribeService));

        /// <summary>
        /// Add a subscriber to the bus
        /// </summary>
        public void Subscribe(ISubscription<BusMessage> subscription)
        {
            if (subscription == null || String.IsNullOrEmpty(subscription.QueueAddress))
            {
                _logger.Error("Subscription was invalid (null, empty queue address or invalid type).");
                return;
            }

            _logger.Debug("SubscriptionService: Subscribe().");

            // Add the subscriber
            Bus.AddSubscriber(subscription);
        }

        /// <summary>
        /// Remove a subscriber from the bus
        /// </summary>
        public void Unsubscribe(ISubscription<BusMessage> subscription)
        {
            if (subscription == null || String.IsNullOrEmpty(subscription.QueueAddress))
            {
                _logger.Error("Subscription was invalid (null, empty queue address or invalid type).");
                return;
            }

            _logger.Debug("SubscriptionService: Unsubscribe().");

            Bus.RemoveSubscriber(subscription);
        }

        #region Hidden

        /// <summary>
        /// This methods does not make sense to use on the service side
        /// </summary>
        /// <param name="busMessage"></param>
        public void OnMessagePublished(IPublish busMessage)
        {
            throw new NotImplementedException("Please don't invoke this method.");
        }

        #endregion
    }
}
