using System;

namespace ED.Blobs
{
    public static class BlobUtils
    {
        public static string GetHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", String.Empty);
        }
    }
}
