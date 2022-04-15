using System.Security.Cryptography.X509Certificates;

namespace ED.Blobs
{
    sealed class X509Certificate2Utils
    {
        public static X509Certificate2? LoadX509Certificate(
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
    }
}
