using System.Runtime.Serialization;

namespace OpenBus.Messages.Events
{
    /// <summary>
    /// Indicates a change in temperature.
    /// </summary>
    [DataContract]
    public class TemperatureEvent : Event
    {
        /// <summary>
        /// The current temperature value.
        /// </summary>
        [DataMember]
        public double Temperature { set; get; }
    }
}