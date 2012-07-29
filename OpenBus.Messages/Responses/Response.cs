using System.Runtime.Serialization;

namespace OpenBus.Messages.Responses
{
    /// <summary>
    /// A response to a request.
    /// </summary>
    [DataContract]
    public abstract class Response : BusMessage
    {

    }
}