using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Blobs
{
    public class PdfServicesClient
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.Cyrillic)
            };

        private readonly HttpClient httpClient;

        public PdfServicesClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<SignatureInfo[]> ExtractSignaturesAsync(
            Stream pdfStream,
            string fileName,
            int blobId,
            CancellationToken ct)
        {
            using MultipartFormDataContent multipartFormDataContent = new();

            using StreamContent pdfContent = new(pdfStream);
            multipartFormDataContent.Add(pdfContent, "pdf");

            using StringContent blobInfoContent =
                new(
                    JsonSerializer.Serialize(
                        new
                        {
                            FileName = fileName,
                            BlobId = blobId,
                        },
                        jsonSerializerOptions),
                    Encoding.UTF8,
                    "application/json");
            multipartFormDataContent.Add(blobInfoContent, "blobInfo");

            HttpResponseMessage response;
            string body;
            try
            {
                response = await this.httpClient.PostAsync(
                    "/api/signatures/extract",
                    multipartFormDataContent,
                    ct);

                body = await response.Content.ReadAsStringAsync(ct);
            }
            catch (HttpRequestException ex)
            {
                throw new PdfServicesClientException(
                    "PdfServices request failed.",
                    ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new PdfServicesClientException($"Non successful status code recieved.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            return JsonSerializer.Deserialize<SignatureInfo[]>(
                body,
                jsonSerializerOptions)
                ?? throw new PdfServicesClientException("Null json returned as body from PdfServices");
        }
    }
}
