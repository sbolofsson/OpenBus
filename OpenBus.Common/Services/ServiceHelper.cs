using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Xml;
using log4net;
using OpenBus.Common.Contracts;
using OpenBus.Common.Security;
using OpenBus.Common.Types;

namespace OpenBus.Common.Services
{
    public static class ServiceHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceHelper));
        private static readonly Dictionary<string, Type> ServiceTypes = new Dictionary<string, Type>();

        private static readonly object MyLock = new object();

        static ServiceHelper()
        {
            Initialize();
        }

        #region Private methods

        /// <summary>
        /// Determines whether a type is a valid system type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsValidSystemType(Type type)
        {
            return (type != null && !ServiceTypes.ContainsKey(type.FullName) &&
                    (type.FullName.Contains("SerializationHolder") ||
                     type.FullName.Contains("System.Type") ||
                     type.FullName.Contains("System.RuntimeType") ||
                     type.FullName.Contains("System.Reflection.RuntimeMethodInfo")));
        }

        /// <summary>
        /// Ensures that all types are in memory
        /// </summary>
        private static void Initialize()
        {
            // If we are not, then get mutual exclusion
            Logger.Info("Initializing ServiceHelper.");

            // Add normal subscriptions
            List<Type> types = SubscriptionValidator.GetPossibleSubscriptions() ?? new List<Type>();

            // Add system types
            types.AddRange(TypeHelper.GetSubTypes(typeof(SystemMessage), true));

            // Certificate
            types.Add(typeof(X509Certificate2));

            // Add framework types
            types.AddRange(new List<Type>
                               {
                                    typeof(Byte),
                                    typeof(SByte),
                                    typeof(Int16),
                                    typeof(Int32),
                                    typeof(Int64),
                                    typeof(UInt16),
                                    typeof(UInt32),
                                    typeof(UInt64),
                                    typeof(Single),
                                    typeof(Double),
                                    typeof(Boolean),
                                    typeof(Char),
                                    typeof(Decimal),
                                    typeof(Object),
                                    typeof(String),
                                    typeof(ArrayList),
                                    typeof(Dictionary<,>),
                                    typeof(DateTime),
                                    typeof(DateTimeOffset),
                                    typeof(TimeSpan),
                                    typeof(Guid),
                                    typeof(Uri),
                                    typeof(XmlQualifiedName),
                                    typeof(List<>),
                                    typeof(Func<,>), 
                                    typeof(Func<,,>), 
                                    typeof(Func<,,,>), 
                                    typeof(Func<,,,,>), 
                                    typeof(Func<,,,,,>), 
                                    typeof(Func<,,,,,,>), 
                                    typeof(Func<,,,,,,,>), 
                                    typeof(Func<,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,,,,,>), 
                                    typeof(Func<,,,,,,,,,,,,,,,,>),
                                    typeof(Action<>),
                                    typeof(Action<,>), 
                                    typeof(Action<,,>), 
                                    typeof(Action<,,,>), 
                                    typeof(Action<,,,,>), 
                                    typeof(Action<,,,,,>), 
                                    typeof(Action<,,,,,,>), 
                                    typeof(Action<,,,,,,,>), 
                                    typeof(Action<,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,,,,,>), 
                                    typeof(Action<,,,,,,,,,,,,,,,>)
                               });

            // Add all types to dictionary
            types.ForEach(t =>
            {
                if (!ServiceTypes.ContainsKey(t.FullName))
                    ServiceTypes.Add(t.FullName, t);
                else
                    Logger.Warn(String.Format("Could not add type '{0}' to service type because it is already added.", t.FullName));
            });

            Logger.Info("ServiceHelper finished initializing.");

        }

        /// <summary>
        /// Checks whether a type is valid and adds it to the known types if it is valid.
        /// </summary>
        /// <param name="type"></param>
        private static void CheckAndAddType(Type type)
        {
            if (type == null)
            {
                Logger.Warn("CheckAndAddType() got null type.");
                return;
            }

            // Is it one of those ugly hidden system types?
            lock (MyLock)
            {
                if (IsValidSystemType(type) && !ServiceTypes.ContainsKey(type.FullName))
                {
                    ServiceTypes.Add(type.FullName, type);
                    Logger.Info(String.Format("CheckAndAddType() added special system type '{0}'.", type.FullName));
                    return;
                }

                // Is it a generic beast?
                while (type != null)
                {
                    if (type.IsGenericType && ServiceTypes.ContainsValue(type.GetGenericTypeDefinition())
                        && !ServiceTypes.ContainsKey(type.FullName)
                        )
                    {
                        Logger.Info(String.Format("CheckAndAddType() added special generic type '{0}'.", type.FullName));
                        ServiceTypes.Add(type.FullName, type);
                    }
                    type = type.BaseType;
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets a valid type for the services on the bus based on its name.
        /// If no valid type is found, null is returned.
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        public static Type GetServiceValidType(string typeStr)
        {
            if (String.IsNullOrEmpty(typeStr))
            {
                Logger.Error("Could not get service valid type for null/empty string.");
                return null;
            }

            Type type;

            // Check the dictionary
            if (ServiceTypes.TryGetValue(typeStr, out type))
                return type;

            // Try to remove special characters from recorded types
            lock (MyLock)
            {
                type = ServiceTypes.Values.SingleOrDefault(t => Encoding.Encoder.SafeEncode(t.FullName) == typeStr);
            }
            if (type != null)
            {
                // Add the type with encoded name !
                string encodedTypeName = Encoding.Encoder.SafeEncode(type.FullName);
                lock (MyLock)
                {
                    if (!ServiceTypes.ContainsKey(encodedTypeName))
                        ServiceTypes.Add(encodedTypeName, type);
                    if (!ServiceTypes.ContainsKey(typeStr))
                        ServiceTypes.Add(typeStr, type);
                }

                return type;
            }

            // Try to decode the type name we are seeking
            string decodedTypeName = Encoding.Encoder.SafeDecode(typeStr);
            lock (MyLock)
            {
                type = ServiceTypes.Values.SingleOrDefault(t => t.FullName == decodedTypeName);
            }
            if (type != null)
            {
                // Add the type with decoded name !
                lock (MyLock)
                {
                    if (!ServiceTypes.ContainsKey(decodedTypeName))
                        ServiceTypes.Add(decodedTypeName, type);
                    if (!ServiceTypes.ContainsKey(typeStr))
                        ServiceTypes.Add(typeStr, type);
                }
                return type;
            }

            // Try using Type.GetType() on normal type name
            Logger.Info(String.Format("Trying to get service valid type for normal type name '{0}' using Type.GetType().", typeStr));
            CheckAndAddType(Type.GetType(typeStr));
            if (ServiceTypes.TryGetValue(typeStr, out type))
                    return type;

            // Try using Type.GetType() on normal type name
            Logger.Info(String.Format("Trying to get service valid type for decoded type name '{0}' using Type.GetType().", decodedTypeName));
            CheckAndAddType(Type.GetType(decodedTypeName));
            if (ServiceTypes.TryGetValue(decodedTypeName, out type))
                    return type;

            if (type == null)
                Logger.Error(String.Format("Could not get service valid type for type name '{0}'.", typeStr));

            return type;
        }

        /// <summary>
        /// Determines whether a type is valid type to use in the services on the bus.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsServiceValidType(Type type)
        {
            if (type == null)
            {
                Logger.Error("Could not determine whether a null type is a valid service type.");
                return false;
            }

            return GetServiceValidType(type.FullName) != null;
        }

        /// <summary>
        /// Gets the known types for a service contract. Excludes generic types.
        /// This method is called by WCF at run time.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetServiceKnownTypes(ICustomAttributeProvider provider)
        {
            return GetServiceKnownTypes(false);
        }

        /// <summary>
        /// Gets the known types for a service contract. Excludes generic types.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetServiceKnownTypes()
        {
            return GetServiceKnownTypes(false);
        }

        /// <summary>
        /// Gets the known types for a service contract.
        /// </summary>
        /// <param name="includeGenericSubTypes">Specifies whether generics should be include</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetServiceKnownTypes(bool includeGenericSubTypes)
        {
            return !includeGenericSubTypes ? ServiceTypes.Values.Where(t => !t.IsGenericType).ToList() : ServiceTypes.Values.ToList();
        }

        #region Communication related methods

        /// <summary>
        /// Determines whether a service host is open
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <returns></returns>
        public static bool IsServiceHostOpen(ServiceHost serviceHost)
        {
            return (serviceHost != null &&
                    (serviceHost.State == CommunicationState.Opened || serviceHost.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Determines whether a channel factory is open
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelFactory"></param>
        /// <returns></returns>
        public static bool IsChannelFactoryOpen<T>(ChannelFactory<T> channelFactory)
        {
            return (channelFactory != null &&
                    (channelFactory.State == CommunicationState.Opened || channelFactory.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Determines whether a client channel is open
        /// </summary>
        /// <param name="clientChannel"></param>
        /// <returns></returns>
        public static bool IsClientChannelOpen(IClientChannel clientChannel)
        {
            return (clientChannel != null &&
                    (clientChannel.State == CommunicationState.Opened || clientChannel.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Closes a service host
        /// </summary>
        /// <param name="serviceHost"></param>
        public static void CloseServiceHost(ServiceHost serviceHost)
        {
            if (serviceHost != null && IsServiceHostOpen(serviceHost))
            {
                try
                {
                    serviceHost.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not close ServiceHost properly.", ex);
                }
            }
            else
                Logger.Error("Could not close ServiceHost since it was either null or it was already closed or closing.");
        }

        /// <summary>
        /// Closes a channel factory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelFactory"></param>
        public static void CloseChannelFactory<T>(ChannelFactory<T> channelFactory)
        {
            if (channelFactory != null && IsChannelFactoryOpen<T>(channelFactory))
            {
                try
                {
                    channelFactory.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not close ChannelFactory properly.", ex);
                }
            }
            else
                Logger.Error("Could not close ChannelFactory since it was either null or it was already closed or closing.");
        }

        /// <summary>
        /// Closes a client channel
        /// </summary>
        /// <param name="clientChannel"></param>
        public static void CloseClientChannel(IClientChannel clientChannel)
        {
            if (clientChannel != null && IsClientChannelOpen(clientChannel))
            {
                try
                {
                    clientChannel.Close();
                    clientChannel.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not close IClientChannel properly.", ex);
                }
            }
            else
                Logger.Error("Could not close IClientChannel since it was either null or it was already closed or closing.");
        }

        #endregion
    }
}
