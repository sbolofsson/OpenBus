using System;
using System.Configuration;
using OpenBus.Common;
using OpenBus.Common.Contracts;
using log4net;
using OpenBus.Bus.Services;
using System.ServiceModel;
using OpenBus.Common.Queues;
using OpenBus.Common.Services;

namespace OpenBus.Bus.Servers
{
    public class Server : IServer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Server));

        private ServiceHost _publishServiceHost;
        private ServiceHost _subscribeServiceHost;

        private string _publishQueueName;
        private string _subscribeQueueName;

        /// <summary>
        /// Standard constructor
        /// </summary>
        public Server()
        {
            Initialize();
        }

        /// <summary>
        /// Initalizes the MSMQ server
        /// </summary>
        private void Initialize()
        {
            try
            {
                _publishQueueName = ConfigurationManager.AppSettings[Constants.Bus.Msmq.Queues.PublishQueue];
                _subscribeQueueName = ConfigurationManager.AppSettings[Constants.Bus.Msmq.Queues.SubscribeQueue];
            }
            catch (Exception ex)
            {
                _logger.Error("Could not start MSMQ server. Check your app.config", ex);
            }
        }

        /// <summary>
        /// Starts the MSMQ server
        /// </summary>
        public void Start()
        {
            // Ensure queues exist
            QueueManager.EnsureQueueExists(_publishQueueName);
            QueueManager.EnsureQueueExists(_subscribeQueueName);

            _logger.Info("Starting publishing service.");

            try
            {
                _publishServiceHost = new ServiceHost(typeof(PublishService));
                _publishServiceHost.Open();
                _logger.Info("Publishing service started.");
            }
            catch (Exception ex)
            {
                _logger.Error("Could not start publishing service.", ex);
            }

            _logger.Info("Starting subscription service.");

            try
            {
                _subscribeServiceHost = new ServiceHost(typeof(SubscribeService));
                _subscribeServiceHost.Open();
                _logger.Info("Subscription service started.");
            }
            catch (Exception ex)
            {
                _logger.Error("Could not start subscription service.", ex);
            }

        }

        /// <summary>
        /// Stops the MSMQ server 
        /// </summary>
        public void Stop()
        {
            _logger.Info("Stopping MSMQ server");
            ServiceHelper.CloseServiceHost(_publishServiceHost);
            ServiceHelper.CloseServiceHost(_subscribeServiceHost);
            _logger.Info("MSMQ stopped");
        }

    }
}
