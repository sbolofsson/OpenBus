using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates that the light has changed.
    /// </summary>
    [DataContract]
    public class LightEvent : Event
    {
        /// <summary>
        /// The current state of the light.
        /// </summary>
        [DataMember]
        public ToggleState State { get; set; }
    }
}