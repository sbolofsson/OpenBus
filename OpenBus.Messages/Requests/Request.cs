using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Request for something to be done or changed.
    /// </summary>
    [DataContract]
    public abstract class Request : BusMessage
    {

    }
}