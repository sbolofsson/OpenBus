using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using log4net;

namespace OpenBus.Common.Serialization
{
    /// <summary>
    /// Custom attribute for services offering better serialization mechanisms.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class BusDataContractFormat : Attribute, IOperationBehavior, IServiceBehavior, IContractBehavior
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BusDataContractFormat));

        #region Implemented methods

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            SetCustomBehavior(description);
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            SetCustomBehavior(description);
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            SetCustomBehavior(serviceDescription);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            SetCustomBehavior(serviceDescription);
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        void IContractBehavior.AddBindingParameters( ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            SetCustomBehavior(contractDescription);
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            SetCustomBehavior(contractDescription);
        }

        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        private static void SetCustomBehavior(ServiceDescription description)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
                SetCustomBehavior(endpoint);
        }

        private static void SetCustomBehavior(ContractDescription description)
        {
            foreach (OperationDescription operation in description.Operations)
                SetCustomBehavior(operation);
        }

        private static void SetCustomBehavior(ServiceEndpoint endpoint)
        {
            // Ignore Mex
            if (endpoint.Contract.ContractType == typeof(IMetadataExchange))
                return;

            SetCustomBehavior(endpoint.Contract);
        }

        #endregion

        /// <summary>
        /// Sets the serialization behavior of WCF to use custom serialization behaviour.
        /// </summary>
        /// <param name="description"></param>
        private static void SetCustomBehavior(OperationDescription description)
        {
            Logger.Debug("BusDataContractFormat: SetCustomBehavior().");

            DataContractSerializerOperationBehavior dataContractSerializerOperationBehavior = description.Behaviors.Find<DataContractSerializerOperationBehavior>();

            if (dataContractSerializerOperationBehavior != null)
            {
                Logger.Debug("BusDataContractFormat: Removing old behavior and adding custom data contract resolver.");

                // Remove old behavior
                description.Behaviors.Remove(dataContractSerializerOperationBehavior);

                // Add custom data contract resolver
                dataContractSerializerOperationBehavior.DataContractResolver = new BusDataContractResolver();
            }

            // Add custom behavior
            description.Behaviors.Add(new BusDataContractSerializerOperationBehavior(description));
        }

        /// <summary>
        /// Class that exposes the custom serializer.
        /// </summary>
        class BusDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
        {
            public BusDataContractSerializerOperationBehavior(OperationDescription operationDescription) : base(operationDescription) { }

            /// <summary>
            /// Gets called by WCF when a serializer is needed.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <param name="ns"></param>
            /// <param name="knownTypes"></param>
            /// <returns></returns>
            public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
            {
                // Return custom data contract serializer.
                Logger.Debug("BusDataContractSerializerOperationBehavior: CreateSerializer() 1.");
                return XmlSerializer.GetDataContractSerializer(type);
            }

            /// <summary>
            /// Gets called by WCF when a serializer is needed.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <param name="ns"></param>
            /// <param name="knownTypes"></param>
            /// <returns></returns>
            public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
            {
                // Return custom data contract serializer.
                Logger.Debug("BusDataContractSerializerOperationBehavior: CreateSerializer() 2.");
                return XmlSerializer.GetDataContractSerializer(type);
            }
        }
    }
}
