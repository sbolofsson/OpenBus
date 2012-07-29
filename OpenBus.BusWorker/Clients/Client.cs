using System;
using System.ServiceModel.Activation;
using OpenBus.Common.Contracts;
using System.Configuration;
using log4net;
using System.ServiceModel;
using OpenBus.Common;
using OpenBus.Common.Serialization;
using OpenBus.Common.Queues;
using OpenBus.Common.Services;
using OpenBus.Messages;

namespace OpenBus.BusWorker.Clients
{
    // Very important that InstanceContextMode = Single because of callback
    // Else the auto-generated instances will not have any references to the callback !
    
    /// <summary>
    /// Class for hosting the application 'client' which communicates directly with the bus
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple),
        //ReleaseServiceInstanceOnTransactionComplete = false),
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    internal class Client : IPublisher<BusMessage>, ISubscriber<BusMessage>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Client));

        private ChannelFactory<IPublisher<BusMessage>> _publishFactory;
        private ChannelFactory<ISubscriber<BusMessage>> _subscribeFactory;
        private IPublisher<BusMessage> _publishProxy;
        private ISubscriber<BusMessage> _subscribeProxy;

        private string _myQueueName;
        private string _myQueueAddress;

        private ServiceHost _callbackServiceHost;
        private static Action<IPublish> _onMessagePublished;

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="onMessagePublished"></param>
        public Client(Action<IPublish> onMessagePublished)
        {
            _onMessagePublished = onMessagePublished;
            Initialize();
        }

        /// <summary>
        /// WCF needs an explicit empty constructor
        /// </summary>
        public Client()
        {
            
        }

        /// <summary>
        /// Initializes the MSMQ client
        /// </summary>
        private void Initialize()
        {
            try
            {
                _myQueueName = ConfigurationManager.AppSettings[Constants.Bus.Msmq.Queues.MyQueue];
                _myQueueAddress = ConfigurationManager.AppSettings[Constants.Bus.Msmq.Addresses.MyQueueAddress];
            }
            catch (Exception ex)
            {
                _logger.Error("Client: Could not start MSMQ client. Check your app.config", ex);
                return;
            }

            QueueManager.EnsureQueueExists(_myQueueName);

            try
            {
                // Setup publishing proxy
                _publishFactory = new ChannelFactory<IPublisher<BusMessage>>("Publisher");
                _publishProxy = _publishFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                _logger.Error("Client: Could not create publish proxy.", ex);
            }

            try
            {
                // Setup subscription proxy
                _subscribeFactory = new ChannelFactory<ISubscriber<BusMessage>>("Subscriber");
                _subscribeProxy = _subscribeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                _logger.Error("Client: Could not create subscribe proxy.", ex);
            }

            // Setup call back service
            try
            {
                //_callbackServiceHost = new ServiceHost(this);
                _callbackServiceHost = new ServiceHost(typeof(Client));
                _callbackServiceHost.Open();
            }
            catch (Exception ex)
            {
                _logger.Error("Client: Could not start call back service.", ex);
                return;
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="busMessage">The message to publish.</param>
        public void Publish(IPublish busMessage)
        {
            if (_publishProxy != null && busMessage != null)
            {
                _logger.Debug("Client: _publishProxy.Publish()");
                try
                {
                    busMessage.Message.Application1Time = DateTime.Now;
                    _publishProxy.Publish(busMessage);
                    _logger.Debug("Client: Successfully published.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Client: Could not publish.", ex);
                }
            }
            else
                _logger.Error("Client: Could not publish because MSMQ client was not properly initialized or message is null.");
        }

        /// <summary>
        /// Subscribes to a message.
        /// </summary>
        /// <param name="subscription"></param>
        public void Subscribe(ISubscription<BusMessage> subscription)
        {
            if (_subscribeProxy != null && subscription != null)
            {
                subscription.QueueAddress = _myQueueAddress;

                _logger.Debug("Client: _subscribeProxy.Subscribe()");
                try
                {
                    _subscribeProxy.Subscribe(subscription);
                    _logger.Debug("Client: Successfully sent subscription.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Client: Could not subscribe.", ex);
                }
            }
            else
                _logger.Error("Client: Could not subscribe because client was not properly initialized or subscription is null.");
        }

        /// <summary>
        /// Unsubscribe from a message
        /// </summary>
        /// <param name="subscription"></param>
        public void Unsubscribe(ISubscription<BusMessage> subscription)
        {
            if (_subscribeProxy != null && subscription != null)
            {
                _logger.Debug("Client: _subscribeProxy.Unsubscribe()");
                try
                {
                    _subscribeProxy.Unsubscribe(subscription);
                    _logger.Debug("Client: Successfully sent unsubscription.");
                }
                catch (Exception ex)
                {
                    _logger.Error("Client: Could not unsubscribe.", ex);
                }
            }
            else
                _logger.Error("Client: Could not unsubscribe because client was not properly initialized or subscription is null.");
        }

        /// <summary>
        /// Gets called when a message is published.
        /// </summary>
        /// <param name="publish"></param>
        //[OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public void OnMessagePublished(IPublish publish)
        {
            if (_onMessagePublished != null)
            {
                _logger.Debug("Calling OnMessagePublished() on client.");
                _onMessagePublished.Invoke(publish);
            }
            else
                _logger.Error("OnMessagePublished() was null.");
        }

        /// <summary>
        /// Releases all communication objects
        /// </summary>
        public void Dispose()
        {
            _logger.Info("Disposing client.");

            // Close publish channel
            ServiceHelper.CloseClientChannel(_publishProxy as IClientChannel);

            // Close subscribe channel
            ServiceHelper.CloseClientChannel(_subscribeProxy as IClientChannel);

            // Close publish factory
            ServiceHelper.CloseChannelFactory(_publishFactory);

            // Close subscribe factory
            ServiceHelper.CloseChannelFactory(_subscribeFactory);

            // Close callback service hose
            ServiceHelper.CloseServiceHost(_callbackServiceHost);

            _logger.Info("Client disposed.");
        }
    }
}
