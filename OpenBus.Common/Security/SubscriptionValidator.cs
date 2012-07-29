using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using OpenBus.Common.Contracts;
using OpenBus.Messages;

namespace OpenBus.Common.Security
{
    /// <summary>
    /// Class for validating if types are valid subscriptions.
    /// </summary>
    public static class SubscriptionValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SubscriptionValidator));
        
        /// <summary>
        /// Determines whether a type is a valid subscription.
        /// </summary>
        /// <param name="type">The type to determine.</param>
        /// <returns>A bool indicating whether the type is valid.</returns>
        public static bool IsValidSubscription(Type type)
        {
            return (type != null &&
                (type.IsSubclassOf(typeof(BusMessage)) || type == typeof(BusMessage)) &&
                (!type.IsSubclassOf(typeof(SystemMessage)) && type != typeof(SystemMessage)));
        }

        /// <summary>
        /// Gets a list of types that are possible to subscribe to. Includes generic types.
        /// </summary>
        /// <returns>A list of valid types</returns>
        public static List<Type> GetPossibleSubscriptions()
        {
            List<Type> types = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(IsValidSubscription).ToList());
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Some types could not be added from assembly '{0}'", assembly.FullName), ex);
                }
            }

            // Insert the top level object
            types.Insert(0, typeof(BusMessage));

            return types;
        }
    }
}
