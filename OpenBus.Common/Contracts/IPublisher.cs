using System.ServiceModel;
using OpenBus.Common.Serialization;
using OpenBus.Common.Services;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    [ServiceContract, ServiceKnownType("GetServiceKnownTypes", typeof(ServiceHelper)), BusDataContractFormat]
    public interface IPublisher<in T> where T : BusMessage
    {
        [OperationContract(IsOneWay = true)]
        void Publish(IPublish busMessage);
    }
}