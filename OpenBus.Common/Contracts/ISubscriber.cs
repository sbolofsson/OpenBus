using System.ServiceModel;
using OpenBus.Common.Serialization;
using OpenBus.Common.Services;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    [ServiceContract, ServiceKnownType("GetServiceKnownTypes", typeof(ServiceHelper)), BusDataContractFormat]
    public interface ISubscriber<in T> where T : BusMessage
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(ISubscription<T> subscription);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(ISubscription<T> subscription);

        [OperationContract(IsOneWay = true)]
        void OnMessagePublished(IPublish publish);
    }
}
