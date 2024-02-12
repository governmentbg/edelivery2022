using System.Security.Cryptography.X509Certificates;

namespace ED.Domain
{
    public sealed class X509Certificate2Utils
    {
        public static X509Certificate2? LoadX509CertificateByThumbPrint(
            string storeName,
            StoreLocation storeLocation,
            string thumbprint)
        {
            X509Certificate2? result = default;

            using X509Store store = new(storeName, storeLocation);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection =
                store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    thumbprint,
                    false);

            if (collection.Count > 0)
            {
                result = collection[0];
            }

            store.Close();

            return result;
        }

        public static X509Certificate2? LoadX509CertificateBySubjectName(
            string storeName,
            StoreLocation storeLocation,
            string name)
        {
            X509Certificate2? result = default;

            using X509Store store = new(storeName, storeLocation);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection =
                store.Certificates.Find(
                    X509FindType.FindBySubjectName,
                    name,
                    false);

            if (collection.Count > 0)
            {
                result = collection[0];
            }

            store.Close();

            return result;
        }
    }
}
