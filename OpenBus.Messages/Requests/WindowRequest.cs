using System.Runtime.Serialization;

namespace OpenBus.Messages.Requests
{
    /// <summary>
    /// Changes the state of a window
    /// </summary>
    [DataContract]
    public class WindowRequest : Request
    {
        /// <summary>
        /// The requested state of the window.
        /// </summary>
        [DataMember]
        public PositionState PositionState { get; set; }
    }
}