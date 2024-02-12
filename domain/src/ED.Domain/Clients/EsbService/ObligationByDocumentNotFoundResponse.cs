using System.Text.Json.Serialization;

namespace ED.Domain
{
    public class ObligationByDocumentNotFoundResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
