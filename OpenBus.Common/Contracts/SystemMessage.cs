using System.Runtime.Serialization;

namespace OpenBus.Common.Contracts
{
    /// <summary>
    /// Top class for restricted messages.
    /// Derived classes of this class are system messages and therefore not valid subscriptions !
    /// </summary>
    [DataContract]
    public abstract class SystemMessage
    {
    }
}
