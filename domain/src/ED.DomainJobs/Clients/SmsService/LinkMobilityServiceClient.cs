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

        public async Task<string?> SendSmsAsync(
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

            using var response = await this.httpClient.PostAsync("/send", content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new LinkMobilityException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<SmsResponse>(body);

            int? smsServiceMetaCode = jsonResponse?.meta?.code;

            if (smsServiceMetaCode != 200 && smsServiceMetaCode != 201)
            {
                throw new LinkMobilityException($"Non successful meta code received.\nMetaCode: {smsServiceMetaCode}\nBody: {body}");
            }

            if (jsonResponse?.data?["sms_id"].ToString() == null)
            {
                throw new LinkMobilityException($"Blank or null sms id returned");
            }

            return jsonResponse.data["sms_id"].ToString();
        }

        public async Task<string?> SendViberAsync(
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
                throw new LinkMobilityException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<ViberResponse>(body);

            int? viberServiceMetaCode = jsonResponse?.meta?.code;

            if (viberServiceMetaCode != 200 && viberServiceMetaCode != 201)
            {
                throw new LinkMobilityException($"Non successful meta code received.\nMetaCode: {viberServiceMetaCode}\nBody: {body}");
            }

            if (jsonResponse?.data?["sms_id"].ToString() == null)
            {
                throw new LinkMobilityException($"Blank or null viber id returned");
            }

            return jsonResponse.data["sms_id"].ToString();
        }

        public async Task SendSmsDeliveryRequest(string smsId, CancellationToken ct)
        {
            SmsDeliveryRequest deliveryRequest = new()
            {
                sms_id = smsId,
                service_id = this.options.ServiceId
            };

            string jsonDeliveryData = JsonSerializer.Serialize(deliveryRequest);

            string hmac = GetHmacAsHexString(
                Encoding.UTF8.GetBytes(jsonDeliveryData),
                Encoding.UTF8.GetBytes(this.options.ApiSecret));

            using StringContent deliveryContent = new(jsonDeliveryData, Encoding.UTF8, "application/json");
            deliveryContent.Headers.Add("x-api-key", this.options.ApiKey);
            deliveryContent.Headers.Add("x-api-sign", hmac);

            using var deliveryResponse = await this.httpClient.PostAsync("/dlr", deliveryContent, ct);
            var deliveryBody = await deliveryResponse.Content.ReadAsStringAsync(ct);

            if (!deliveryResponse.IsSuccessStatusCode)
            {
                throw new LinkMobilityException(
                    $"Non successful status code received.\nStatusCode: {deliveryResponse.StatusCode}\nBody: {deliveryBody}");
            }

            var jsonDeliveryResponse = JsonSerializer.Deserialize<SmsResponse>(deliveryBody);

            int? smsStatus = int.Parse(jsonDeliveryResponse!.data!["status"].ToString()!);

            if (smsStatus != 201 && smsStatus != 202)
            {
                throw new LinkMobilityException(
                    $"Non successful sms delivery : {jsonDeliveryResponse!.data!["sms_id"]} | {jsonDeliveryResponse!.data!["status_msg"]}");
            }
        }

        public async Task SendViberDeliveryRequest(string viberId, CancellationToken ct)
        {
            ViberDeliveryRequest deliveryRequest = new()
            {
                sms_id = viberId,
                service_id = this.options.ServiceId
            };

            string jsonDeliveryData = JsonSerializer.Serialize(deliveryRequest);

            string hmac = GetHmacAsHexString(
                Encoding.UTF8.GetBytes(jsonDeliveryData),
                Encoding.UTF8.GetBytes(this.options.ApiSecret));

            using StringContent deliveryContent = new(jsonDeliveryData, Encoding.UTF8, "application/json");
            deliveryContent.Headers.Add("x-api-key", this.options.ApiKey);
            deliveryContent.Headers.Add("x-api-sign", hmac);

            using var deliveryResponse = await this.httpClient.PostAsync("/dlr", deliveryContent, ct);
            var deliveryBody = await deliveryResponse.Content.ReadAsStringAsync(ct);

            if (!deliveryResponse.IsSuccessStatusCode)
            {
                throw new LinkMobilityException(
                    $"Non successful status code received.\nStatusCode: {deliveryResponse.StatusCode}\nBody: {deliveryBody}");
            }

            var jsonDeliveryResponse = JsonSerializer.Deserialize<ViberResponse>(deliveryBody);

            int? viberStatus = int.Parse(jsonDeliveryResponse!.data!["status"].ToString()!);

            if (viberStatus != 201 && viberStatus != 202)
            {
                throw new LinkMobilityException(
                    $"Non successful viber delivery : {jsonDeliveryResponse!.data!["sms_id"]} | {jsonDeliveryResponse!.data!["status_msg"]}");
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
