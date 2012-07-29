using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Request the alarm to change state, e.g. turn on alarm
    /// </summary>
    [DataContract]
    public class AlarmRequest : Request
    {
        /// <summary>
        /// The requested state of the alarm.
        /// </summary>
        [DataMember]
        public ToggleState State { get; set; }

        /// <summary>
        /// If a password is required set it here.
        /// </summary>
        [DataMember]
        public string Password { get; set; }
    }
}