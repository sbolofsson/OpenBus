﻿using System.Security.Cryptography.X509Certificates;
using OpenBus.Messages;

namespace OpenBus.Common.Contracts
{
    public class Publish : SystemMessage, IPublish
    {
        /// <summary>
        /// A description of the certificate to include when publishing.
        /// </summary>
        public Certificate Certificate { get; set; }

        /// <summary>
        /// The actual certificate, will be set by the middleware.
        /// </summary>
        public X509Certificate2 X509Certificate { get; set; }

        /// <summary>
        /// The message to be published.
        /// </summary>
        public BusMessage Message { get; set; }
    }
}
