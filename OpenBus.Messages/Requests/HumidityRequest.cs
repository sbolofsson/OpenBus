using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Request for changing the humidity
    /// </summary>
    [DataContract]
    public class HumidityRequest : Request
    {
        /// <summary>
        /// The requested humidity setpoint.
        /// </summary>
        [DataMember]
        public double SetPoint { get; set; }
    }
}
