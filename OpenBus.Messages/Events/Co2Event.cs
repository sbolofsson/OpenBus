using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates a change in Co2 level.
    /// </summary>
    [DataContract]
    public class Co2Event : Event
    {
        /// <summary>
        /// The current Co2 level.
        /// </summary>
        [DataMember]
        public double Co2 { set; get; }
    }
}