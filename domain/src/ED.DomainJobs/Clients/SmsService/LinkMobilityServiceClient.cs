using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class LinkMobilityServiceClient
    {
        private readonly HttpClient httpClient;
        private readonly LinkMobilityOptions options;

        public LinkMobilityServiceClient(
            HttpClient httpClient,
            IOptions<ClientsOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value.LinkMobility;
        }
        public async Task SendSmsAsync(
            string phoneNumber,
            string smsText,
            CancellationToken ct)
        {
            SmsRequest data = new()
            {
                msisdn = phoneNumber,
                text = smsText.Truncate(this.options.Sms.MaxMessageSize),
                sc = this.options.Sms.Sc,
                service_id = this.options.ServiceId
            };

            string jsonData = JsonSerializer.Serialize(data);

            string hmac = GetHmacAsHexString(
                Encoding.UTF8.GetBytes(jsonData),
                Encoding.UTF8.GetBytes(this.options.ApiSecret));

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");
            content.Headers.Add("x-api-key", this.options.ApiKey);
            content.Headers.Add("x-api-sign", hmac);

            using var response = await this.httpClient.PostAsync("/bulknew", content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new LinkMobilityException($"Non successful status code recieved.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<SmsResponse>(body);

            int? smsServiceMetaCode = jsonResponse?.meta?.code;

            if (smsServiceMetaCode != 200 && smsServiceMetaCode != 201)
            {
                throw new LinkMobilityException($"Non successful meta code recieved.\nMetaCode: {smsServiceMetaCode}\nBody: {body}");
            }
        }

        public async Task SendViberAsync(
            string phoneNumber,
            string viberText,
            CancellationToken ct)
        {
            ViberRequest data = new()
            {
                msisdn = phoneNumber,
                text = viberText,
                sc = this.options.Viber.Sc,
                service_id = this.options.ServiceId,
                fallback = new()
                {
                    sms = viberText.Truncate(this.options.Sms.MaxMessageSize),
                }
            };

            string jsonData = JsonSerializer.Serialize(data);

            string hmac = GetHmacAsHexString(
                Encoding.UTF8.GetBytes(jsonData),
                Encoding.UTF8.GetBytes(this.options.ApiSecret));

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");
            content.Headers.Add("x-api-key", this.options.ApiKey);
            content.Headers.Add("x-api-sign", hmac);

            using var response = await this.httpClient.PostAsync("/send", content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new LinkMobilityException($"Non successful status code recieved.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<ViberResponse>(body);

            int? viberServiceMetaCode = jsonResponse?.meta?.code;

            if (viberServiceMetaCode != 200 && viberServiceMetaCode != 201)
            {
                throw new LinkMobilityException($"Non successful meta code recieved.\nMetaCode: {viberServiceMetaCode}\nBody: {body}");
            }
        }

        private static string GetHmacAsHexString(byte[] data, byte[] secret)
        {
            using HMACSHA512 hmac = new(secret);
            byte[] binaryHmac = hmac.ComputeHash(data);
            return BitConverter.ToString(binaryHmac).Replace("-", "").ToLower();
        }
    }
}
