using System.Collections.Generic;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace ED.Domain
{
    public class OrnServiceClient
    {
        private const string IdType = "EMessages";

        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new()
            {
                Encoder = JavaScriptEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Cyrillic)
            };

        private static class OrnQueryParams
        {
            public const string Bulstat = "bulstat";
            public const string IdType = "id_type";
            public const string UriService = "uri_service";
            public const string OidSystem = "oid_system";
        }

        private readonly HttpClient httpClient;
        private string systemOId;

        public OrnServiceClient(
            HttpClient httpClient,
            IOptions<DomainOptions> domainOptionsAccessor)
        {
            this.httpClient = httpClient;
            this.systemOId = domainOptionsAccessor.Value.SystemOId
                ?? throw new DomainException($"Missing {nameof(DomainOptions)}.{nameof(DomainOptions.SystemOId)} setting.");
        }

        public class OrnResponse
        {
            [JsonPropertyName("return_code")]
            public string ReturnCode { get; set; } = null!;

            [JsonPropertyName("description_code")]
            public string DescriptionCode { get; set; } = null!;

            [JsonPropertyName("orn")]
            public string Orn { get; set; } = null!;
        }

        public async Task<string> SubmitAsync(
            string identifier,
            CancellationToken ct)
        {
            Dictionary<string, string?> queryString = new Dictionary<string, string?>()
            {
                [OrnQueryParams.Bulstat] = identifier,
                [OrnQueryParams.IdType] = IdType,
                [OrnQueryParams.UriService] = "1234", // TODO
                [OrnQueryParams.OidSystem] = this.systemOId,
            };

            using HttpRequestMessage request = new(
                HttpMethod.Get,
                QueryHelpers.AddQueryString("/WebServiceUniqueNumber/rest/orn", queryString));

            request.Headers.Add(HeaderNames.Accept, "application/json");

            HttpResponseMessage? response = null;
            string body;

            try
            {
                response = await this.httpClient.SendAsync(request, ct);

                body = await response.Content.ReadAsStringAsync(ct);
            }
            catch (HttpRequestException ex)
            {
                OrnException networkOrnException =
                    new($"OrnService request failed. StatusCode: {response!.StatusCode}", ex);

                throw networkOrnException;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new OrnException($"Non successful status code recieved.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            OrnResponse ornResponse =
                JsonSerializer.Deserialize<OrnResponse>(body, jsonSerializerOptions)
                    ?? throw new OrnException("Null json returned as body from OrnServices");

            OrnException? validateOrnException = this.ValidateResponse(ornResponse);

            if (validateOrnException != null)
            {

                throw validateOrnException;
            }

            return ornResponse.Orn;
        }

        private OrnException? ValidateResponse(
            OrnResponse response)
        {
            if (string.IsNullOrEmpty(response.Orn))
            {
                return new OrnException("Empty orn");
            }

            return null;
        }
    }
}
