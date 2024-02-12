#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming Styles

namespace ED.Domain
{
    public class TokenResponse
    {
        public string token_type { get; set; } = null!;

        public string access_token { get; set; } = null!;

        public long expires_in { get; set; }

        public long consented_on { get; set; }

        public string scope { get; set; } = null!;

        public string grant_type { get; set; } = null!;
    }
}
