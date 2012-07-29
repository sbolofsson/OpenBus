using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// A request for Co2 to be changed.
    /// </summary>
    [DataContract]
    public class Co2Request : Request
    {
        /// <summary>
        /// The requested Co2 setpoint.
        /// </summary>
        [DataMember]
        public double SetPoint { get; set; }
    }
}
