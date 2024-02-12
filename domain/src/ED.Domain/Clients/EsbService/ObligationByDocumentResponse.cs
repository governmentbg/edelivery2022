using System.Text.Json.Serialization;

namespace ED.Domain
{
    public class ObligationByDocumentResponse
    {
        [JsonPropertyName("accessCode")]
        public string AccessCode { get; set; } = null!;
    }
}
