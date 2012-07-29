using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates whether it started or stopped raining.
    /// </summary>
    [DataContract]
    public class RainEvent : Event
    {
        /// <summary>
        /// Is it raining.
        /// </summary>
        [DataMember]
        public bool IsRaining { get; set; }
    }
}