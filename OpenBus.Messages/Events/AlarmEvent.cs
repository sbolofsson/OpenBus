using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates a change in the alarm.
    /// </summary>
    [DataContract]
    public class AlarmEvent : Event
    {
        /// <summary>
        /// The current state of the alarm.
        /// </summary>
        [DataMember]
        public ToggleState AlarmState { get; set; }
    }
}