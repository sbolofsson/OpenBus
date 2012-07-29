using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace OpenBus.Common.Types
{
    /// <summary>
    /// Helper class for working with types
    /// </summary>
    public static class TypeHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TypeHelper));

        /// <summary>
        /// Get all valid sub classes of a certain class.
        /// </summary>
        /// <param name="messageType">The type of the busmessage.</param>
        /// <param name="includeParent">Specifies whether the parent type should be included in the list.</param>
        /// <returns>A list of types.</returns>
        public static List<Type> GetSubTypes(Type messageType, bool includeParent)
        {
            List<Type> types = new List<Type>();
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(messageType)).ToList());
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Some types could not be added from assembly '{0}'", assembly.FullName), ex);
                }
            }

            if (includeParent)
                types.Insert(0, messageType);

            return types;
        }

        /// <summary>
        /// Tests whether a type is of another generic type.
        /// </summary>
        /// <param name="genericType">The generic type to test</param>
        /// <param name="target">The type to validate against</param>
        /// <returns></returns>
        public static bool IsOfGenericType(Type genericType, Type target)
        {
            while (target != null)
            {
                if (target.IsGenericType &&
                    target.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
                target = target.BaseType;
            }
            return false;
        }

    }
}
