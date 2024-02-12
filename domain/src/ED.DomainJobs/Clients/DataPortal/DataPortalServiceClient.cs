using System;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class DataPortalServiceClient
    {
        private readonly HttpClient httpClient;
        private readonly DataPortalOptions options;

        private static readonly JsonSerializerOptions JsonSerializerOptions =
            new()
            {
                Encoder = JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.Cyrillic)
            };

        public DataPortalServiceClient(
            HttpClient httpClient,
            IOptions<DataPortalOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        public async Task<string> PostProfilesMonthyStatisticsAsync(
            string? datasetUri,
            IJobsDataPortalListQueryRepository.GetProfilesMonthlyStatisticsVO[] statistics,
            DateTime from,
            DateTime to,
            CancellationToken ct)
        {
            string? newDatasetUri = null;

            if (string.IsNullOrWhiteSpace(datasetUri))
            {
                newDatasetUri = await this.AddDatasetAsync(
                    this.options.ApiKey,
                    this.options.OrganizationId,
                    "bg",
                    "Списък на  регистрираните лица по целевите групи през Системата за сигурно електронно връчване",
                    7,
                    "Списък на  регистрираните лица по целевите групи през Системата за сигурно електронно връчване",
                    1,
                    ct);
            }

            string newResourceUri = await this.AddResourceMetadataAsync(
                this.options.ApiKey,
                newDatasetUri ?? datasetUri!,
                "bg",
                 $"Списък/Регистър профили за {from:dd.MM.yyyy}-{to:dd.MM.yyyy}",
                 "csv",
                 1,
                ct);

            string[][] arrData =
                this.GetDynamicDataForProfilesMonthyStatistics(statistics);

            _ = await this.AddorUpdateResourceAsync(
                true,
                this.options.ApiKey,
                newResourceUri,
                "csv",
                arrData,
                ct);

            return newDatasetUri ?? datasetUri!;
        }

        private string[][] GetDynamicDataForProfilesMonthyStatistics(
            IJobsDataPortalListQueryRepository.GetProfilesMonthlyStatisticsVO[] statistics)
        {
            string[][] array = new string[statistics.Length + 1][];
            array[0] = new string[]
            {
                "Целева група",
                "Наименование",
                "Брой изпратени съобщения през ССЕВ",
                "Брой получени съобщения през ССЕВ",
            };

            for (int i = 0; i < statistics.Length; i++)
            {
                IJobsDataPortalListQueryRepository.GetProfilesMonthlyStatisticsVO current = statistics[i];

                array[i + 1] = new string[]
                {
                    current.TargetGroupName,
                    current.Name,
                    current.SentMessagesCount.ToString(),
                    current.ReceivedMessagesCount.ToString(),
                };
            }

            return array;
        }

        private async Task<string> AddDatasetAsync(
            string apiKey,
            int organizationId,
            string locale,
            string name,
            int categoryId,
            string description,
            int visibility,
            CancellationToken ct)
        {
            AddDatasetRequest data = new()
            {
                api_key = apiKey,
                data = new AddDatasetRequestData
                {
                    org_id = organizationId,
                    locale = locale,
                    name = name,
                    category_id = categoryId,
                    description = description,
                    visibility = visibility,
                }
            };

            string jsonData = JsonSerializer.Serialize(data, JsonSerializerOptions);

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using var response = await this.httpClient.PostAsync("addDataset", content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new DataPortalException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<AddDatasetResponse>(body);

            return jsonResponse!.uri;
        }

        private async Task<string> AddResourceMetadataAsync(
            string apiKey,
            string datasetUri,
            string locale,
            string name,
            string fileFormat,
            int type,
            CancellationToken ct)
        {
            AddResourceMetadataRequest data = new()
            {
                api_key = apiKey,
                dataset_uri = datasetUri,
                data = new AddResourceMetadataRequestData
                {
                    locale = locale,
                    name = name,
                    file_format = fileFormat,
                    type = type,
                }
            };

            string jsonData = JsonSerializer.Serialize(data, JsonSerializerOptions);

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using var response = await this.httpClient.PostAsync("addResourceMetadata", content, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new DataPortalException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<AddResourceMetadataResponse>(body);

            return jsonResponse!.data.uri;
        }

        private async Task<bool> AddorUpdateResourceAsync(
            bool add,
            string apiKey,
            string resourceUri,
            string extensionFormat,
            string[][] arrData,
            CancellationToken ct)
        {
            AddOrUpdateResourceRequest data = new()
            {
                api_key = apiKey,
                resource_uri = resourceUri,
                extension_format = extensionFormat,
                data = arrData,
            };

            string jsonData = JsonSerializer.Serialize(data, JsonSerializerOptions);

            using StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            using var response = await this.httpClient.PostAsync(
                add ? "addResourceData" : "updateResourceData",
                content,
                ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new DataPortalException($"Non successful status code received.\nStatusCode: {response.StatusCode}\nBody: {body}");
            }

            var jsonResponse = JsonSerializer.Deserialize<AddOrUpdateResourceResponse>(body);

            return jsonResponse!.success;
        }
    }
}
