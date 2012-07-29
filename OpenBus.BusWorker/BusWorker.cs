using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using OpenBus.BusWorker.Clients;
using OpenBus.Common.Contracts;
using System.Reflection;
using log4net;
using OpenBus.Common.Security;
using OpenBus.Common.Transformation;
using OpenBus.Messages;

namespace OpenBus.BusWorker
{
    /// <summary>
    /// Abstract class defining a mixed publisher/subscriber.
    /// </summary>
    public abstract class BusWorker
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(BusWorker));

        // 'Synchronous' publishing and monitor synchronization
        private readonly object _sync;
        private readonly object _signal;
        private bool _isFirst;
        private BusMessage _callbackMessage;

        // Timers used for scheduled publishing 
        private readonly Dictionary<string, Timer> _timers;

        // Communication related variables
        private Dictionary<Type, ISubscription<BusMessage>> _subscriptions;
        private readonly Client _client;

        // Lock for subscriptions
        private readonly object _subscriptionLock = new object();

        /// <summary>
        /// Standard constructor
        /// </summary>
        protected BusWorker()
        {
            // Initialize dictionary for callback handlers
            _subscriptions = new Dictionary<Type, ISubscription<BusMessage>>();

            // Initialize variables for 'synchronous' publish
            _sync = new object();
            _signal = new object();
            _isFirst = false;

            // Initialize variables for scheduled publishing
            _timers = new Dictionary<string, Timer>();

            // Initialize the client to handle wcf
            _logger.Info("Setting up client");
            _client = new Client(OnMessagePublished);
        }

        /// <summary>
        /// Destructor disposes connections
        /// </summary>
        ~BusWorker()
        {
            Dispose();
        }

        #region Publish methods

        /// <summary>
        /// Publishes a message to the bus.
        /// </summary>
        /// <param name="busMessage">The message to publish.</param>
        public void Publish(BusMessage busMessage)
        {
            Publish(new Publish { Message = busMessage});
        }

        /// <summary>
        /// Publishes a message to the bus.
        /// </summary>
        /// <param name="publish">A description of the publish.</param>
        public void Publish(Publish publish)
        {
            if (publish == null || publish.Message == null)
            {
                _logger.Error("Could not publish null message.");
                return;
            }

            // Try to get certificate
            Certificate certificate = publish.Certificate;
            if(certificate != null)
            {
                X509Certificate2 x509Certificate2 = CertificateHelper.GetCertificate(publish.Certificate);
                if(x509Certificate2 == null)
                    throw new ArgumentException("Could not find a matching certificate. Please make sure it exists in your trusted store under the local machine.");

                publish.X509Certificate = x509Certificate2;
            }

            // Map local to global id
            string resourceId = publish.Message.ResourceId;
            if (!String.IsNullOrEmpty(resourceId))
            {
                publish.Message.MessageId = GuidMapper.GetGuid(resourceId);
                publish.Message.ResourceId = null; // Hide the local id for other applications
            }

            _logger.Info(String.Format("Publishing message:\n{0}\n", publish.Message));

            try
            {
                _client.Publish(publish);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not publish message.", ex);
            }
        }

        /// <summary>
        /// Publishes a message to the bus at a specific schedule.
        /// </summary>
        /// <param name="publishMethod">The message publishing method to schedule.</param>
        /// <param name="firstPublish">The time to make the first publish. Should be null or DateTime.MinValue if it should start from now.</param>
        /// <param name="scheduledTime">The timespan schedule.</param>
        /// <param name="name">The name for the scheduled publishing - use this if you want to be able to take it down again later.</param>
        public void Publish(Action publishMethod, DateTime firstPublish, TimeSpan scheduledTime, string name)
        {
            if (publishMethod == null || String.IsNullOrEmpty(name))
            {
                _logger.Error("Could not setup publish because name or method was null.");
                return;
            }

            // Should we publish now.
            long firstSchedule = 0;

            if (firstPublish != DateTime.MinValue)
            {
                TimeSpan timeDifference = firstPublish.Subtract(DateTime.Now);

                // No worries about overflow. Max value of this can become
                // 59 * 1000
                // 59 * 60 * 1000
                // 23 * 60 * 60 * 1000
                // 999
                // = 86.399.999
                //
                // And Int64.MaxValue = 9.223.372.036.854.775.807
                firstSchedule = (
                                (timeDifference.Seconds * 1000) +
                                (timeDifference.Minutes * 60 * 1000) +
                                (timeDifference.Hours * 60 * 60 * 1000) +
                                (timeDifference.Days * 24 * 60 * 60 * 1000) +
                                (timeDifference.Milliseconds)
                                );
            }

            // Then we publish every timeSpan milliseconds.
            long timeSpan = (
                            (scheduledTime.Seconds * 1000) +
                            (scheduledTime.Minutes * 60 * 1000) +
                            (scheduledTime.Hours * 60 * 60 * 1000) +
                            (scheduledTime.Days * 24 * 60 * 60 * 1000) +
                            (scheduledTime.Milliseconds)
                            );

            Timer timer;

            try
            {
                timer = new Timer(AsyncPublish, publishMethod, firstSchedule, timeSpan);
            }
            catch (Exception ex)
            {
                _logger.Error("Could not setup automatic publishing.", ex);
                return;
            }

            _timers.Add(name, timer);
        }

        /// <summary>
        /// Publishes a message to the bus at a specific schedule.
        /// </summary>
        /// <param name="publishMethod">The message publishing method to schedule.</param>
        /// <param name="firstPublish">The time to make the first publish. Should be null or DateTime.MinValue if it should start from now.</param>
        /// <param name="scheduledTime">The timespan schedule.</param>
        public void Publish(Action publishMethod, DateTime firstPublish, TimeSpan scheduledTime)
        {
            if (publishMethod == null)
            {
                _logger.Error("Could not setup scheduled publish when the publish method is null.");
                return;
            }

            // Set up publish with random guid as name
            Publish(publishMethod, firstPublish, scheduledTime, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Removes a scheduled publish
        /// </summary>
        /// <param name="name">The name of the scheduled publish.</param>
        public void RemoveScheduledPublish(string name)
        {
            Timer timer;
            lock (_timers)
            {
                if (_timers.TryGetValue(name, out timer))
                {
                    if (timer == null)
                    {
                        _logger.Error(String.Format("Could not remove timer with name '{0}'.", name));
                        return;
                    }

                    try
                    {
                        timer.Dispose();
                        _timers.Remove(name);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(String.Format("Could not dispose timer with name '{0}'.", name), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Used to invoke the scheduled publishing method asynchronously.
        /// Gets called by the run time environment.
        /// </summary>
        /// <param name="state">The function parameter.</param>
        private void AsyncPublish(object state)
        {
            Action publishMethod = state as Action;

            if (publishMethod != null)
            {
                try
                {
                    publishMethod.BeginInvoke(null, null);
                }
                catch (Exception ex)
                {
                    _logger.Error("Could not invoke scheduled publish.", ex);
                }
            }
            else
                _logger.Error("Scheduled publish method was null.");
        }
        
        public T PublishAndWait<T>(BusMessage busMessage, uint timeout, bool matchId) where T : BusMessage
        {
            if (busMessage == null)
            {
                _logger.Error("Cannot publish null message.");
                return default(T);
            }
            return PublishAndWait<T>(new Publish { Message = busMessage }, timeout, matchId);
        }

        /// <summary>
        /// Publishes a message to the bus and waits for a specific message to occur with a timeout.
        /// This method will block until either the message occurs or it times out.
        /// </summary>
        /// <typeparam name="T">The type of the message we expect to get back.</typeparam>
        /// <param name="publish">The message to publish.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="matchId">Specifies whether the Id of the returned message should be matched with the published message's Id.</param>
        /// <returns>The specified type.</returns>
        public T PublishAndWait<T>(Publish publish, uint timeout, bool matchId) where T : BusMessage
        {
            if (publish == null || publish.Message == null)
            {
                _logger.Error("Cannot publish null message or with a negative timeout.");
                return null;
            }

            // Subscribe to the callback if we are not already subscribed
            bool subscribed = false;

            Type key = typeof(T);

            if (_subscriptions.ContainsKey(key))
            {
                _logger.Info(String.Format("PublishAndWait: A subscription was already made for message type '{0}'.", key));
                subscribed = true;
            }
            else
            {
                _logger.Info(String.Format("PublishAndWait: Subscribing to message type '{0}'.", key));
                _client.Subscribe(new Subscription<T>());
            }

            // Enter the monitor
            lock (_sync)
            {
                // The idea is:
                // 1. Register a new callback handler for the type.
                // 2. When that callbackHandler gets hit it signals that the global message is ready to be returned.
                // 3. Now we can return the requested type.

                // We don't want an old message so erase global message.
                _callbackMessage = null;

                // Flag that we only need a single handler
                _isFirst = true;

                // Possibly store the original handler
                ISubscription<BusMessage> originalSubscription = null;
                lock (_subscriptions)
                {
                    if (_subscriptions.ContainsKey(key))
                    {
                        _logger.Info(String.Format("PublishAndWait: Storing original callback handler for type '{0}'.", key.FullName));
                        originalSubscription = _subscriptions[key];
                    }

                    // Set a new handler no matter what
                    _logger.Info(String.Format("PublishAndWait: Registering special callback handler for type '{0}'.", key.FullName));
                    _subscriptions[key] = new Subscription<BusMessage> { CallbackHandler = OnPublishCallback };
                }

                // Publish
                // NOTE: In theory the publish could get an answer immediately thus waiting unnecessary,
                // also we could get multiple answers before we get to restore the original handler ..
                // Also we could miss the first reply and get the next instead.
                _logger.Info(String.Format("PublishAndWait: Publishing message of type '{0}'.", key.FullName));
                Publish(publish);

                // Wait
                _logger.Info("Waiting for response.");
                Monitor.Wait(_sync, (int)timeout);

                // ... light years later

                // Possibly restore the original handler
                if (originalSubscription != null)
                {
                    _logger.Info(String.Format("PublishAndWait: Restoring callback handler for type '{0}'.", key));
                    _subscriptions[key] = originalSubscription;
                }
                else // or just remove the new one
                {
                    _logger.Info(String.Format("PublishAndWait: Removing special callback handler for type '{0}'.", key));
                    _subscriptions.Remove(key);
                }

                // Possibly unsubscribe
                if (!subscribed)
                {
                    _logger.Info(String.Format("PublishAndWait: Unsubscribing from message type '{0}'.", key));
                    _client.Unsubscribe(new Subscription<T>());
                }

                // Signal possible waiting callers of OnMessagePublishedCallback.
                lock (_signal)
                {
                    _logger.Info("PublishAndWait: Signalling possible excessive amount of responses.");
                    Monitor.PulseAll(_signal);

                    // Hopefully we have something, if not then return
                    if (_callbackMessage == null)
                        return default(T);
                    
                    Guid busMessageId = publish.Message.MessageId;
                    Guid returnedMessageId = _callbackMessage.MessageId;

                    // Match on id ?
                    if (matchId)
                    {
                        // Is there a match?
                        if (busMessageId != Guid.Empty && returnedMessageId != Guid.Empty && busMessageId == returnedMessageId)
                        {
                            _logger.Info(String.Format("Successfully received a message with a match on id. Published message id: '{0}', received message id: '{1}'", busMessageId, returnedMessageId));
                            return _callbackMessage as T;
                        }

                        // No match or empty guids
                        _logger.Error(String.Format("Failed to receive a message with a match on id. Published message id: '{0}', received message id: '{1}'", busMessageId, returnedMessageId));
                        return default(T);
                    }

                    // Return the message and leave the monitors
                    return _callbackMessage as T;
                }
            }
        }

        #endregion

        #region Subscribe methods

        /// <summary>
        /// Subscribes and registers a callback handler.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="callbackHandler">The callback handler to register.</param>
        public void Subscribe<T>(Action<T> callbackHandler) where T : BusMessage
        {
            if (callbackHandler == null)
            {
                _logger.Error("Could not subscribe to message with null callback handler.");
                return;
            }

            Subscribe(new Subscription<T> { CallbackHandler = callbackHandler });
        }

        /// <summary>
        /// Subscribes and registers an callback handler.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="subscription"></param>
        public void Subscribe<T>(Subscription<T> subscription) where T : BusMessage
        {
            if (subscription == null || subscription.CallbackHandler == null)
            {
                _logger.Error("Could not subscribe to message with null subscription or null callback handler.");
                return;
            }

            _client.Subscribe(subscription);

            lock (_subscriptionLock)
            {
                Type key = typeof(T);
                if (!_subscriptions.ContainsKey(key))
                    _subscriptions.Add(key, subscription);
            }
        }

        /// <summary>
        /// Unsubscribes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>() where T : BusMessage
        {
            Unsubscribe(new Subscription<T>());
        }

        /// <summary>
        /// Unsubscribes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscription"></param>
        public void Unsubscribe<T>(Subscription<T> subscription) where T : BusMessage
        {
            if (subscription == null)
            {
                _logger.Error("Could not unsubscribe with null subscription.");
                return;
            }

            _client.Unsubscribe(subscription);
            _subscriptions.Remove(typeof(T));
        }

        /// <summary>
        /// Unsubscribe from everything that is subscribed to on the bus.
        /// </summary>
        public void UnsubscribeAll()
        {
            lock (_subscriptionLock)
            {
                // NOTE: This step is necessary in order not to violate the enumeration constraints
                List<Type> subscriptions = _subscriptions.Keys.ToList();

                subscriptions.ForEach(t =>
                {
                    try
                    {
                        _logger.Info(String.Format("Unsubscribing from '{0}'.", t.FullName));
                        HandleSubscription(Unsubscribe<BusMessage>, t);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(String.Format("Could not unsubscribe from message of type '{0}'.", t.FullName), ex);
                    }
                });

                // Make sure to reset the subscriptions record
                _subscriptions = new Dictionary<Type, ISubscription<BusMessage>>();
            }
        }

        /// <summary>
        /// Gets the list of types currently subscribed to.
        /// </summary>
        /// <returns>A list of types.</returns>
        public List<Type> GetSubscriptions()
        {
            return _subscriptions.Keys.ToList();
        }

        /// <summary>
        /// Handles a subscription.
        /// If type is not known at compile time this method will ensure that only subscribers to types deriving from 'BusMessage' will be handled
        /// </summary>
        /// <param name="handler">The handler to invoke.</param>
        /// <param name="type"></param>
        private void HandleSubscription(Action handler, Type type)
        {
            if (handler == null || type == null)
            {
                _logger.Error("Could not register callback handler since it is not null or message type is null.");
                return;
            }

            // This will fail if the string messageType violates our type constrain
            MethodInfo genericMethod = null;
            try
            {
                genericMethod = handler.Method.GetGenericMethodDefinition().MakeGenericMethod(type);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Could not add subscriber to type '{0}' since it is not a legal type.", type), ex);
            }

            // Now try to invoke the method
            if (genericMethod != null)
            {
                try
                {
                    genericMethod.Invoke(this, new object[] { });
                }
                catch (Exception ex)
                {
                    _logger.Error(String.Format("Subscription was not handled for type '{0}'.", type.FullName), ex);
                }
            }
        }

        #endregion

        #region Callback handling

        /// <summary>
        /// Gets called when a subscribed message is published.
        /// It passes the message on to the derived class asynchronously.
        /// Do not use this method from the 'outside'.
        /// </summary>
        /// <param name="publish">The message that has been published.</param>
        private void OnMessagePublished(IPublish publish)
        {
            if (publish == null || publish.Message == null)
            {
                _logger.Error("No callback handler was invoked because received message was null.");
                return;
            }

            ISubscription<BusMessage> subscription;

            Type type = publish.Message.GetType();

            if (_subscriptions.TryGetValue(type, out subscription))
            {
                if (subscription != null)
                {
                    // Convert from global id to local id
                    publish.Message.ResourceId = GuidMapper.GetLocalId(publish.Message.MessageId);

                    subscription.InvokeCallbackHandler(publish);
                }
                else
                    _logger.Error(String.Format("Callback handler for type '{0}' was null.", type.FullName));
            }
            else
                _logger.Error(String.Format("No callback handler has been assigned to react on message of type '{0}'.", type.FullName));

        }

        /// <summary>
        /// Gets called when a subscribed message is published.
        /// It sets a handle signalling a waiting method to proceed.
        /// This method is used for 'synchronous' publishing.
        /// </summary>
        /// <param name="busMessage">The message that has been published.</param>
        private void OnPublishCallback<T>(T busMessage) where T : BusMessage
        {
            _logger.Info("OnPublishCallback() was called.");

            // Enter the monitor.
            // The reason for using 2 monitors is that:
            // we want to make sure we signal to the caller of PublishAndWait and not a caller of OnPublishCallback;
            lock (_signal)
            {
                // In this region can only be one thread at the time !

                lock (_sync)
                {
                    // If we are the first one then we should set the global message.
                    if (_isFirst)
                    {
                        _logger.Info("OnPublishCallback: I am first, so I set the global message.");
                        _isFirst = false;

                        // Set the shared message
                        _callbackMessage = busMessage;

                        // Then signal the waiting publish method and leave both monitors
                        _logger.Info("OnPublishCallback: Signalling waiting BusWorker.");
                        Monitor.Pulse(_sync);
                        return;
                    }
                    
                    _logger.Info("OnPublishCallback: I am not the first one. Calling OnMessagePublished().");
                    // Else we wait until the original callback handler has been reset
                    // and then handle the message regularly again
                    Monitor.Wait(_signal);
                    OnMessagePublished(new Publish { Message = busMessage});
                }
            }
        }

        #endregion

        /// <summary>
        /// Cleans up WCF connections.
        /// Does not unsubscribe from anything.
        /// </summary>
        public virtual void Dispose()
        {
            // Dispose timers
            if (_timers != null)
            {
                foreach (Timer timer in _timers.Values)
                {
                    try
                    {
                        timer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Could not dispose Timer object.", ex);
                    }
                }
            }

            // Finally dispose client
            if (_client != null)
                _client.Dispose();
        }

        /// <summary>
        /// Creates an instance of a BusWorker.
        /// </summary>
        /// <returns></returns>
        public static BusWorker Create()
        {
            return new Worker();
        }

        /// <summary>
        /// Convenient class for making an instance of a BusWorker.
        /// </summary>
        private class Worker : BusWorker { }
    }
}
