using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class InfosystemsServiceClient
    {
        private const int ViberAndSmsScenarioId = 54; // provided from infosystems
        private const int SmsScenarioId = 55; // provided from infosystems
        private const int ViberScenarioId = 56; // provided from infosystems

        private const int ViberAndSmsPriority = 1;
        private const int SmsPriority = 1;
        private const int ViberPriority = 1;

        private const string ReportMessageChannelSMS = "SMS";
        private const int ReportMessageSuccessfulStatusCode = 100;

        private readonly HttpClient httpClient;
        private readonly InfosystemsOptions options;

        public InfosystemsServiceClient(
            HttpClient httpClient,
            IOptions<InfosystemsOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        public async Task<string?> SendSmsAsync(
            string phoneNumber,
            string smsText,
            CancellationToken ct)
        {
            SmsRequest data = new()
            {
                cid = this.options.Cid,
                sid = SmsScenarioId,
                priority = SmsPriority,
                recipient = new SmsRequestRecipient
                {
                    msisdn = phoneNumber.ToPhone(),
                },
                message = new SmsRequestMessage
                {
                    sms = new SmsRequestMessageText
                    {
                        text = smsText,
                    }
                }
            };

            string jsonData = JsonSerializer.Serialize(data);

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response =
                await this.httpClient.PostAsync("/public/v1/send-single/", content, ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new InfosystemsException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            SmsResponse jsonResponse = JsonSerializer.Deserialize<SmsResponse>(body)
                ?? throw new InfosystemsException($"Response not deserialized.\nBody: {body}");

            string? smsServiceMetaCode = jsonResponse.status;

            if (smsServiceMetaCode == null
                || (!smsServiceMetaCode.Contains("A01") && !smsServiceMetaCode.Contains("A02")))
            {
                throw new InfosystemsException($"Message not accepted.\n{smsServiceMetaCode}\nBody: {body}");
            }

            if (jsonResponse.msgId == 0)
            {
                throw new InfosystemsException($"Blank or null sms id returned");
            }

            return jsonResponse.msgId.ToString();
        }

        public async Task<string?> SendViberAsync(
            string phoneNumber,
            string viberText,
            string? smsText,
            CancellationToken ct)
        {
            ViberRequest data = !string.IsNullOrEmpty(smsText)
                ? new()
                {
                    cid = this.options.Cid,
                    sid = ViberAndSmsScenarioId,
                    priority = ViberAndSmsPriority,
                    recipient = new ViberRequestRecipient
                    {
                        msisdn = phoneNumber.ToPhone(),
                    },
                    message = new ViberRequestRecipientMessage
                    {
                        viber = new ViberRequestRecipientMessageText
                        {
                            text = viberText,
                        },
                        sms = new ViberRequestRecipientMessageText
                        {
                            text = smsText,
                        },
                    }
                }
                : new()
                {
                    cid = this.options.Cid,
                    sid = ViberScenarioId,
                    priority = ViberPriority,
                    recipient = new ViberRequestRecipient
                    {
                        msisdn = phoneNumber.ToPhone(),
                    },
                    message = new ViberRequestRecipientMessage
                    {
                        viber = new ViberRequestRecipientMessageText
                        {
                            text = viberText,
                        },
                    }
                };

            string jsonData = JsonSerializer.Serialize(data);

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response =
                await this.httpClient.PostAsync("/public/v1/send-single/", content, ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new InfosystemsException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            ViberResponse jsonResponse = JsonSerializer.Deserialize<ViberResponse>(body)
                ?? throw new InfosystemsException($"Response not deserialized.\nBody: {body}");

            string? smsServiceMetaCode = jsonResponse.status;

            if (smsServiceMetaCode == null
                || (!smsServiceMetaCode.Contains("A01") && !smsServiceMetaCode.Contains("A02")))
            {
                throw new InfosystemsException($"Message not accepted.\n{smsServiceMetaCode}\nBody: {body}");
            }

            if (jsonResponse.msgId == 0)
            {
                throw new InfosystemsException($"Blank or null viber id returned");
            }

            return jsonResponse.msgId.ToString();
        }

        public async Task<CreateSmsViberDeliveryCommandMessages[]> SendSmsDeliveryRequest(
            string smsId,
            CancellationToken ct)
        {
            using HttpResponseMessage response = await this.httpClient.GetAsync(
                $"/public/v1/report?msgId={smsId}&clientId={this.options.Cid}",
                ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new InfosystemsException(
                    $"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            SmsDeliveryResponse jsonResponse =
                JsonSerializer.Deserialize<SmsDeliveryResponse>(body)
                    ?? throw new InfosystemsException($"Response not deserialized.\nBody: {body}");

            if (!string.IsNullOrEmpty(jsonResponse.error))
                throw new InfosystemsException(
                    $"Non successful sms delivery: {smsId} | {jsonResponse.error}");

            if (jsonResponse.messages == null || jsonResponse.messages.Length == 0)
                throw new InfosystemsException(
                    $"Non successful sms delivery: {smsId} Message status is empty");

            CreateSmsViberDeliveryCommandMessages[] result = jsonResponse
                .messages
                .Select(e => Enumerable.Repeat(
                    new CreateSmsViberDeliveryCommandMessages(
                        e.statusCode == ReportMessageSuccessfulStatusCode
                            ? DeliveryStatus.Delivered
                            : DeliveryStatus.Error,
                        e.charge,
                        e.channel == ReportMessageChannelSMS
                            ? DeliveryResultType.Sms
                            : DeliveryResultType.Viber), 
                    e.messageParts))
                .SelectMany(p => p)
                .ToArray();

            return result;
        }

        public async Task<CreateSmsViberDeliveryCommandMessages[]> SendViberDeliveryRequest(
            string viberId,
            CancellationToken ct)
        {
            using HttpResponseMessage response = await this.httpClient.GetAsync(
                $"/public/v1/report?msgId={viberId}&clientId={this.options.Cid}",
                ct);

            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new InfosystemsException(
                    $"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            ViberDeliveryResponse jsonResponse =
                JsonSerializer.Deserialize<ViberDeliveryResponse>(body)
                    ?? throw new InfosystemsException($"Response not deserialized.\nBody: {body}");

            if (!string.IsNullOrEmpty(jsonResponse.error))
                throw new InfosystemsException(
                    $"Non successful viber delivery: {viberId} | {jsonResponse.error}");

            if (jsonResponse.messages == null || jsonResponse.messages.Length == 0)
                throw new InfosystemsException(
                    $"Non successful viber delivery: {viberId} Message status is empty");

            CreateSmsViberDeliveryCommandMessages[] result = jsonResponse
                .messages
                .Select(e => Enumerable.Repeat(
                    new CreateSmsViberDeliveryCommandMessages(
                        e.statusCode == ReportMessageSuccessfulStatusCode
                            ? DeliveryStatus.Delivered
                            : DeliveryStatus.Error,
                        e.charge,
                        e.channel == ReportMessageChannelSMS
                            ? DeliveryResultType.Sms
                            : DeliveryResultType.Viber),
                    e.messageParts))
                .SelectMany(p => p)
                .ToArray();

            return result;
        }
    }
}
