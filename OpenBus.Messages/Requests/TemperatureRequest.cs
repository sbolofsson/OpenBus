using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Request for the temperature to be set.
    /// </summary>
    [DataContract]
    public class TemperatureRequest : Request
    {
        /// <summary>
        /// The requested temperature.
        /// </summary>
        [DataMember]
        public double SetPoint { get; set; }
    }
}