using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Events indicating that something has changed.
    /// </summary>
    [DataContract]
    public abstract class Event : BusMessage
    {

    }
}