using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Configuration;

namespace EDelivery.SEOS.Utils
{
    public class CertificateHelper
    {
        private static X509Certificate2 seosCertificate = null;

        public static X509Certificate2 SEOSCertificate
        {
            get
            {
                if (seosCertificate == null)
                {
                    var thumbprintOfCertificate =
                        WebConfigurationManager.AppSettings["SEOS.CertificateThumbprint"];
                    seosCertificate = LoadCertificateByThumbprint(thumbprintOfCertificate);
                }
                return seosCertificate;
            }
        }

        private static X509Certificate2 LoadCertificateByThumbprint(string thumbprint)
        {
            var store = new X509Store("MY", StoreLocation.LocalMachine);

            if (String.IsNullOrEmpty(thumbprint))
                throw new NullReferenceException("Thumbprint for SEOS certificate is empty!");

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            var collection = (X509Certificate2Collection)store.Certificates;

            foreach (X509Certificate2 x509 in collection)
            {
                if (!String.IsNullOrEmpty(x509.Thumbprint) && x509.Thumbprint.ToUpper() == thumbprint.ToUpper())
                    return x509;

                x509.Reset();
            }

            throw new ApplicationException($"There is no certificate with the specified thumbprint {thumbprint}!");
        }
    }
}
