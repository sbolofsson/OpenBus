using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates that humidity has changed.
    /// </summary>
    [DataContract]
    public class HumidityEvent : Event
    {
        /// <summary>
        /// The current humidity.
        /// </summary>
        [DataMember]
        public double Humidity { get; set; }
    }
}