using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EDelivery.SEOS.As4RestService
{
    public class As4RestClient
    {
        private readonly HttpClient httpClient;

        public As4RestClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GetPayloadAsync(
            string id, 
            CancellationToken ct)
        {
            var resp = await this.httpClient.GetAsync(
                $"/domibus/ext/messages/{id}/payloads/message",
                ct);

            string body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                return body;
            }
            else
            {
                string message = this.GetError(resp.StatusCode);

                throw new As4RestClientException(
                    message,
                    resp.StatusCode.ToString(),
                    (int)resp.StatusCode,
                    body,
                    null);
            }
        }

        public async Task<string> GetEnvelopeAsync(
            string id, 
            CancellationToken ct)
        {
            var resp = await this.httpClient.GetAsync(
                $"/domibus/ext/messages/usermessages/{id}/envelope",
                ct);

            string body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                return body;
            }
            else
            {
                string message = this.GetError(resp.StatusCode);

                throw new As4RestClientException(
                    message,
                    resp.StatusCode.ToString(),
                    (int)resp.StatusCode,
                    body,
                    null);
            }
        }

        public async Task<bool> DeletePayloadAsync(
            string id,
            CancellationToken ct)
        {
            var resp = await this.httpClient.DeleteAsync(
                $"/domibus/ext/monitoring/messages/finalstatus/delete/{id}", //?mshRole=RECEIVING
                ct);

            string body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                string message = this.GetError(resp.StatusCode);

                throw new As4RestClientException(
                    message,
                    resp.StatusCode.ToString(),
                    (int)resp.StatusCode,
                    body,
                    null);
            }
        }

        private string GetError(HttpStatusCode code)
        {
            switch (code)
            {
                case HttpStatusCode.NotFound:
                    return "An evaluation with the specified ID could not be found.";
                case HttpStatusCode.Unauthorized:
                    return "Authentication is required and/or has failed.";
                case HttpStatusCode.Forbidden:
                    return "The user is not permitted to perform this operation.";
                case HttpStatusCode.InternalServerError:
                    return "An internal error has occurred.";
                default:
                    return "The HTTP status code of the response was not expected";
            }
        }
    }
}
