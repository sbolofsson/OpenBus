using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Request for the light to be set
    /// </summary>
    [DataContract]
    public class LightRequest : Request
    {
        /// <summary>
        /// The requested state of the light.
        /// </summary>
        [DataMember]
        public ToggleState State { get; set; }

    }
}