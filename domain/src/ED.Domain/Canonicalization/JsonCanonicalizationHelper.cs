using Org.Webpki.JsonCanonicalizer;

namespace ED.Domain
{
    public sealed class JsonCanonicalizationHelper
    {
        public static string Canonicalize(string json)
        {
            JsonCanonicalizer jsonCanonicalizer = new(json);

            return jsonCanonicalizer.GetEncodedString();
        }
    }
}
