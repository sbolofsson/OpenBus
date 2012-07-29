using System;
using System.Runtime.Serialization;

namespace OpenBus.Common.Contracts
{
    /// <summary>
    /// A wrapper class for X.509 Certificates
    /// </summary>
    [DataContract]
    public class Certificate : SystemMessage
    {
        /// <summary>
        /// The issuer of the certificate
        /// </summary>
        [DataMember]
        public string Issuer { get; set; }

        /// <summary>
        /// The subject of the certificate
        /// </summary>
        [DataMember]
        public string Subject { get; set; }

        /// <summary>
        /// Specifies when the certificate is valid from
        /// </summary>
        [DataMember]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Specifies when the certificate is valid to
        /// </summary>
        [DataMember]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// The public key of the certificate
        /// </summary>
        [DataMember]
        public string PublicKey { get; set; }

        /// <summary>
        /// The serial number of the certificate
        /// </summary>
        [DataMember]
        public string SerialNumber { get; set; }
    }
}
