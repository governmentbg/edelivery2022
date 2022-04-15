using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class PushNotificationServiceClient
    {
        private readonly HttpClient httpClient;
        private readonly PushNotificationJobOptions options;
        public PushNotificationServiceClient(
            HttpClient httpClient,
            IOptions<DomainJobsOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value.PushNotificationJob;
        }

        public async Task SendPushNotificationAsync(
            string pushNotificationUrl,
            object pushNotification,
            CancellationToken ct)
        {
            if (pushNotification == null)
            {
                throw new ArgumentException("Parameter pushNotification should not be null");
            }

            using var response = await this.httpClient.PostAsJsonAsync(
                pushNotificationUrl,
                pushNotification,
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new PushNotificationException(
                    $"Can not send push notification to url {pushNotificationUrl}. StatusCode {response.StatusCode}. Body {body}");
            }
        }
    }
}
