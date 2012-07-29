using System;
using System.Runtime.Serialization;
using System.Xml;
using log4net;
using OpenBus.Common.Services;

namespace OpenBus.Common.Serialization
{
    /// <summary>
    /// Custom data contract resolver. Is used to validate valid types at runtime
    /// http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractresolver.aspx
    /// </summary>
    public class BusDataContractResolver : DataContractResolver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BusDataContractResolver));

        /// <summary>
        /// Tries to resolve whether a type is known.
        /// Is used for serialization.
        /// Gets called by the framework.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="declaredType"></param>
        /// <param name="knownTypeResolver"></param>
        /// <param name="typeName"></param>
        /// <param name="typeNamespace"></param>
        /// <returns></returns>
        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            Logger.Debug(String.Format("BusDataContractResolver: TryResolveType(type='{0}', declaredType='{1}')", type.FullName, declaredType.FullName));

            if (ServiceHelper.IsServiceValidType(type))
            {
                XmlDictionary dictionary = new XmlDictionary();
                string encodedTypeName = Encoding.Encoder.SafeEncode(type.FullName);
                Logger.Debug(String.Format("BusDataContractResolver: TryResolveType() got valid type: '{0}'. Encoded name: '{1}'.", type.FullName, encodedTypeName));
                typeName = dictionary.Add(encodedTypeName);
                typeNamespace = dictionary.Add("http://tempuri.com");
                return true;
            }

            Logger.Error(String.Format("BusDataContractResolver: TryResolveType() got invalid type: '{0}'.", type.FullName));
            return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
        }

        /// <summary>
        /// Tries to get a type by name.
        /// Is used for deserialization.
        /// Gets called by the framework.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="typeNamespace"></param>
        /// <param name="declaredType"></param>
        /// <param name="knownTypeResolver"></param>
        /// <returns></returns>
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            Logger.Debug(String.Format("BusDataContractResolver: ResolveName(typeName='{0}', typeNamespace='{1}', declaredType='{2}')", typeName, typeNamespace, declaredType.FullName));

            Type type = ServiceHelper.GetServiceValidType(typeName);

            if (type == null)
            {
                Logger.Debug(String.Format("BusDataContractResolver: ResolveName() got invalid type: '{0}'. Trying to get it from declared type: '{1}'.", typeName, declaredType.FullName));
                if (ServiceHelper.IsServiceValidType(declaredType))
                {
                    Logger.Warn(String.Format("BusDataContractResolver: ResolveName() was successful using declared type: '{0}.", declaredType));
                    type = declaredType;
                }
            }

            if(type != null)
            {
                Logger.Debug(String.Format("BusDataContractResolver: ResolveName() got valid type: '{0}'.", typeName));
                return type;
            }

            Logger.Error(String.Format("BusDataContractResolver: ResolveName() got invalid type: '{0}'.", typeName));
            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
        }
    }
}
