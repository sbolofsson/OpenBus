using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenBus.Messages
{
    /// <summary>
    /// Base class defining valid messages on the bus.
    /// All messages that are going to be sent on the bus should directly or indirectly inherit from this class.
    /// </summary>
    [DataContract]
    public abstract class BusMessage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected BusMessage()
        {
            MessageId = Guid.NewGuid();
            Time = DateTime.Now;
        }

        [DataMember]
        public DateTime Application1Time { get; set; }
        [DataMember]
        public DateTime Application2Time { get; set; }
        [DataMember]
        public DateTime PublishServiceTime { get; set; }

        /// <summary>
        /// The local Id for the message.
        /// This will automatically be mapped over to the global Id on publish.
        /// </summary>
        [DataMember]
        public string ResourceId { set; get; }

        /// <summary>
        /// The global Id for the message.
        /// This will automatically be mapped over to the local Id on callback.
        /// </summary>
        [DataMember]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Time of creation
        /// </summary>
        [DataMember]
        public DateTime Time { get; set; }

        /// <summary>
        /// Prints a message well formated.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // No worries, but types are not nullable.
            return String.Format("MessageId: '{0}' Time: '{1}'", MessageId, Time.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
