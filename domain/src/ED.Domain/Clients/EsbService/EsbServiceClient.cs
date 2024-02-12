using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System;
using System.Text;
using System.Globalization;
using System.Net;

namespace ED.Domain
{
    public class EsbServiceClient
    {
        public enum EsbScope
        {
            TicketScope = 0,
            ObligationScope = 1,
        }

        public const int IdStage = 3;
        public const int IdService = 1074;

        public const string TokenPurposeHttpClientName = "EsbServiceClient_TokenPurposeHttpClientName";
        public const string SubmitHttpClientName = "EsbServiceClient_SubmitHttpClientName";

        private readonly HttpClient tokenPurposeHttpClient;
        private readonly HttpClient submitHttpClient;
        private readonly EsbOptions options;

        public EsbServiceClient(
            IHttpClientFactory httpClientFactory,
            IOptions<EsbOptions> options)
        {
            this.tokenPurposeHttpClient =
                httpClientFactory.CreateClient(TokenPurposeHttpClientName);
            this.submitHttpClient =
                httpClientFactory.CreateClient(SubmitHttpClientName);
            this.options = options.Value;
        }

        public async Task<string> GetTokenAsync(
            EsbScope esbScope,
            string? headerRepresentedPersonID,
            string? headerCorrespondentOID,
            string? headerOperatorID,
            CancellationToken ct)
        {
            string scope = this.GetTokenScope(esbScope);

            string tokenUrl =
                $"/token?grant_type=client_credentials&client_id={this.options.ClientId}&scope={scope}";

            this.tokenPurposeHttpClient.DefaultRequestHeaders.Add("representedPersonID", headerRepresentedPersonID ?? string.Empty);
            this.tokenPurposeHttpClient.DefaultRequestHeaders.Add("correspondentOID", headerCorrespondentOID ?? string.Empty);
            this.tokenPurposeHttpClient.DefaultRequestHeaders.Add("operatorID", headerOperatorID ?? string.Empty);
            this.tokenPurposeHttpClient.DefaultRequestHeaders.Add("OID", this.options.Oid);

            using HttpResponseMessage response =
                await this.tokenPurposeHttpClient.PostAsync(tokenUrl, null!, ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new EsbException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            TokenResponse? jsonResponse =
                JsonSerializer.Deserialize<TokenResponse>(body);

            if (jsonResponse == null
                || jsonResponse.access_token == null)
            {
                throw new EsbException($"Blank or null access_token returned");
            }

            return jsonResponse.access_token;
        }

        public record MultipleObligationsDO(
            int Count,
            string? NotFoundMessage);

        public async Task<MultipleObligationsDO> LoadObligationsAsync(
            string token,
            CancellationToken ct)
        {
            this.submitHttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await this.submitHttpClient.GetAsync(
                "/fine/payment/obligationsByPerson",
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ObligationsByPersonNotFoundResponse? jsonNotFoundResponse =
                        JsonSerializer.Deserialize<ObligationsByPersonNotFoundResponse>(body);

                    return new MultipleObligationsDO(0, jsonNotFoundResponse?.Message);
                }

                throw new EsbException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            // if count is really needed then deserialize the body
            return new MultipleObligationsDO(1, null);
        }

        public record SingleObligationDO(
            string? AccessCode,
            string? NotFoundMessage);

        public async Task<SingleObligationDO> GetObligationAccessCodeAsync(
            string token,
            string documentType,
            string documentIdentifier,
            CancellationToken ct)
        {
            this.submitHttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await this.submitHttpClient.GetAsync(
                $"/fine/payment/obligationByDocument?documentType={documentType}&documentIdentifier={documentIdentifier}",
                ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ObligationByDocumentNotFoundResponse? jsonNotFoundResponse =
                        JsonSerializer.Deserialize<ObligationByDocumentNotFoundResponse>(body);

                    return new SingleObligationDO(null, jsonNotFoundResponse?.Message);
                }

                throw new EsbException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            ObligationByDocumentResponse? jsonResponse =
                JsonSerializer.Deserialize<ObligationByDocumentResponse>(body);

            return new SingleObligationDO(jsonResponse!.AccessCode, null);
        }

        public async Task<string> SubmitDeliveredTicketAsync(
            string token,
            IdentifierType identifierType,
            string identifier,
            int ticketId,
            DateTime timestamp,
            CancellationToken ct)
        {
            PostDeliveredTicketRequest data = new()
            {
                IdentifierType = identifierType,
                Identifier = identifier,
                TicketId = ticketId,
                Timestamp = DateOnly
                    .FromDateTime(timestamp)
                    .ToString("o", CultureInfo.InvariantCulture), // ISO 8601 format https://learn.microsoft.com/en-us/dotnet/standard/datetime/how-to-use-dateonly-timeonly#parse-and-format-dateonly
            };

            string jsonData = JsonSerializer.Serialize(data);

            using StringContent content =
                new(jsonData, Encoding.UTF8, "application/json");

            this.submitHttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await this.submitHttpClient.PostAsync(
                "/fine/served",
                content,
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new EsbException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            PostDeliveredTicketResponse? jsonResponse =
                JsonSerializer.Deserialize<PostDeliveredTicketResponse>(body);

            return jsonResponse!.MQmessageID;
        }

        private string GetTokenScope(EsbScope esbScope)
        {
            return esbScope switch
            {
                EsbScope.TicketScope => "/fine*",
                EsbScope.ObligationScope => "/fine*",
                _ => throw new NotImplementedException()
            };
        }
    }
}
