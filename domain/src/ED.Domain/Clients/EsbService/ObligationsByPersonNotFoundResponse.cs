using System.Text.Json.Serialization;

namespace ED.Domain
{
    public class ObligationsByPersonNotFoundResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
