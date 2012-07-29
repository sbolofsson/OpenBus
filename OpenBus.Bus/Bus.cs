using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using OpenBus.Common.Contracts;
using OpenBus.Common.Serialization;
using OpenBus.Common.Types;
using OpenBus.Messages;

namespace OpenBus.Bus
{
    public static class Bus
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Bus));
        private static Dictionary<Type, List<ISubscription<BusMessage>>> _subscribers = new Dictionary<Type, List<ISubscription<BusMessage>>>();
        private static readonly object MyLock = new object();
        private const string SubscriberPath = "/Subscribers.xml";

        static Bus()
        {
            Initialize();
        }

        #region Subscriber handling

        /// <summary>
        /// Gets all subscribers for a given message
        /// </summary>
        /// <param name="messageType">The message to get subscribers for.</param>
        /// <returns>A list of subscribers.</returns>
        public static List<ISubscription<BusMessage>> GetSubscribers(Type messageType)
        {
            if (messageType == null)
            {
                Logger.Error("Could not get subscribers for a null message type.");
                return new List<ISubscription<BusMessage>>();
            }

            Logger.Info(String.Format("Getting subscribers for message type '{0}'", messageType.FullName));

            List<ISubscription<BusMessage>> subscribers = new List<ISubscription<BusMessage>>();
            _subscribers.TryGetValue(messageType, out subscribers);
            return subscribers;
        }

        /// <summary>
        /// Adds a subscriber to a message.
        /// </summary>
        /// <param name="subscription"></param>
        public static void AddSubscriber<T>(ISubscription<T> subscription) where T : BusMessage
        {
            if (subscription == null)
            {
                Logger.Error("Could not add a null subscription.");
                return;
            }

            Type key = subscription.GetType().GetGenericArguments()[0];

            // Should we get the subclasses?
            List<Type> types = (subscription.IncludeSubClasses) ? TypeHelper.GetSubTypes(key, true) : new List<Type> { key };

            // Should we get the generic subclasses?
            if (subscription.IncludeGenericSubClasses)
            {
                Type[] genericTypeArguments = key.GetGenericArguments();
                if (genericTypeArguments.Length == 1)
                {
                    Logger.Debug(String.Format("Making a subscription with generic sub classes for type '{0}'.", key.FullName));
                    List<Type> genericSubClasses = TypeHelper.GetSubTypes(genericTypeArguments[0], false);
                    genericSubClasses.ForEach(t => types.Add(key.MakeGenericType(t)));
                }
                else
                    Logger.Error(String.Format("Trying to add generic subclasses but generic type argument was not found for type '{0}'.", key.FullName));
            }

            lock (MyLock)
            {
                foreach (Type type in types)
                {
                    Logger.Info(String.Format("Trying to add subscriber to message type '{0}'.", type.FullName));

                    List<ISubscription<BusMessage>> subscribers = new List<ISubscription<BusMessage>>();

                    // We assume that, if we have the key, then we have read the xml file.
                    if (_subscribers.TryGetValue(type, out subscribers))
                    {
                        // Do we have the subscriber?
                        if (!subscribers.Exists(s => s.QueueAddress == subscription.QueueAddress))
                        {
                            // No, then add him
                            subscribers.Add(subscription);
                            XmlSerializer.SerializeToFile(_subscribers, SubscriberPath);
                            Logger.Info("Subscriber was added.");
                        }
                        else
                            Logger.Warn("Subscriber was not added because a subscription was already made.");
                    }
                    else
                    {
                        Logger.Info("Subscriber was added as the first one.");
                        _subscribers.Add(type, new List<ISubscription<BusMessage>>{subscription});
                        XmlSerializer.SerializeToFile(_subscribers, SubscriberPath);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a subscriber from an message.
        /// </summary>
        /// <param name="subscription"></param>
        public static void RemoveSubscriber<T>(ISubscription<T> subscription) where T : BusMessage
        {
            if (subscription == null)
            {
                Logger.Error("Could not remove a null subscriber.");
                return;
            }

            Type key = subscription.GetType().GetGenericArguments()[0];

            Logger.Info(String.Format("Trying to remove subscriber from message type '{0}'", key.FullName));

            List<Type> types = (subscription.IncludeSubClasses) ? TypeHelper.GetSubTypes(key, true) : new List<Type> { key };

            lock (MyLock)
            {
                foreach (Type type in types)
                {
                    Logger.Info(String.Format("Trying to add subscriber to message type '{0}'.", key.FullName));

                    List<ISubscription<BusMessage>> subscribers = new List<ISubscription<BusMessage>>();

                    // We assume that, if we have the key, then we have read the xml file.
                    if (_subscribers.TryGetValue(type, out subscribers))
                    {
                        ISubscription<BusMessage> subscriberToRemove = subscribers.SingleOrDefault(s => s.QueueAddress == subscription.QueueAddress);

                        if (subscriberToRemove != null)
                        {
                            subscribers.Remove(subscriberToRemove);
                            XmlSerializer.SerializeToFile(_subscribers, SubscriberPath);
                            Logger.Info("Subscriber was removed");
                        }
                        else
                            Logger.Error("Could not remove subscriber because subscriber does not exist.");
                    }
                    else
                        Logger.Error("Could not remove subscriber because subscriber was not subscribed.");
                }
            }
        }

        private static void Initialize()
        {
            _subscribers = XmlSerializer.DeserializeFromFile<Dictionary<Type, List<ISubscription<BusMessage>>>>(SubscriberPath);

            // Very first time the subscribers will be null
            if(_subscribers == null)
            {
                _subscribers = new Dictionary<Type, List<ISubscription<BusMessage>>>();
                XmlSerializer.SerializeToFile(_subscribers, SubscriberPath);
            }
        }
        #endregion
    }
}
