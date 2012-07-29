using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using log4net;
using OpenBus.Common.Contracts;

namespace OpenBus.Common.Security
{
    public static class CertificateHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CertificateHelper));

        /// <summary>
        /// Checks whether the invoker has a matching certificate in his physical store.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static bool HasCertificate(Certificate certificate)
        {
            if (certificate == null)
                return true;

            List<X509Certificate2> certificates = GetCertificates(certificate);

            Logger.Debug(String.Format("CertificateHelper: HasCertificate matched on '{0}' certificates.", certificates != null ? certificates.Count : 0));

            return (certificates != null && certificates.Count == 1);
        }

        /// <summary>
        /// Gets a single certificate if a perfect match is found.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificate(Certificate certificate)
        {
            if (certificate == null)
                return null;

            List<X509Certificate2> certificates = GetCertificates(certificate);

            Logger.Debug(String.Format("CertificateHelper: GetCertificate matched on '{0}' certificates.", certificates != null ? certificates.Count : 0));

            return (certificates != null && certificates.Count == 1) ? certificates.First() : null;
        }

        /// <summary>
        /// Gets the physical certificates that match the input certificate from the Local Machine and Trusted People store.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static List<X509Certificate2> GetCertificates(Certificate certificate)
        {
            // If the certificate is null then we have none matching ?
            if (certificate == null)
                return null;

            X509Store x509Store;

            try
            {
                x509Store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
            }
            catch (Exception ex)
            {
                Logger.Error("Could not access certificate store.", ex);
                return null;
            }

            return x509Store.Certificates.Cast<X509Certificate2>().Where(c => (ContainsCondition(c.Issuer, certificate.Issuer)) &&
                                                                              (EqualCondition(c.GetPublicKeyString(), certificate.PublicKey)) &&
                                                                              (EqualCondition(c.GetSerialNumberString(), certificate.SerialNumber)) &&
                                                                              (ContainsCondition(c.Subject, certificate.Subject)) &&
                                                                              (EqualCondition(c.GetEffectiveDateString(), certificate.ValidFrom != DateTime.MinValue ? certificate.ValidFrom.ToString() : null)) &&
                                                                              (EqualCondition(c.GetExpirationDateString(), certificate.ValidTo != DateTime.MinValue ? certificate.ValidTo.ToString() : null))).ToList();
        }

        private static bool EqualCondition(string value, string condition)
        {
            // If the condition is empty, then we do not check anything !
            if (String.IsNullOrEmpty(condition))
                return true;

            // If the value is empty and the condition is not, then we do not satisfy the condition !
            if (String.IsNullOrEmpty(value))
                return false;

            return value == condition;
        }

        private static bool ContainsCondition(string value, string condition)
        {
            // If the condition is empty, then we do not check anything !
            if (String.IsNullOrEmpty(condition))
                return true;

            // If the value is empty and the condition is not, then we do not satisfy the condition !
            if (String.IsNullOrEmpty(value))
                return false;

            return value.Contains(condition);
        }
    }
}
