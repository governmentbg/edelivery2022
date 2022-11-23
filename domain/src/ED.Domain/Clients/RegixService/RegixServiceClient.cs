using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Options;
using RegixClient;

namespace ED.Domain
{
    public class RegixServiceClient
    {
        private static string RegixDateFormat = "yyyy-MM-dd";
        private readonly RegixOptions options;

        public RegixServiceClient(IOptions<RegixOptions> options)
        {
            this.options = options.Value;
        }

        public async Task<RegixPersonInfoResponse?> GetRegixPersonInfoAsync(
            string identifier,
            CancellationToken ct)
        {
            try
            {
                WSHttpBinding binding = new(SecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
                binding.MaxReceivedMessageSize = 10000000;

                string endpointOption =
                    this.options.Settings.Endpoint
                        ?? throw new Exception("Missing RegixSettingsEndpoint option");

                EndpointAddress endpoint = new(endpointOption);

                using RegiXEntryPointClient client = new(binding, endpoint);

                string serviceOP = "TechnoLogica.RegiX.GraoNBDAdapter.APIService.INBDAPI.ValidPersonSearch";
                string requestXml = $@"
<ValidPersonRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                    xmlns=""http://egov.bg/RegiX/GRAO/NBD/ValidPersonRequest"">
    <EGN>{identifier}</EGN>
</ValidPersonRequest>";
                XmlDocument xDoc = new();
                xDoc.LoadXml(requestXml);

                ServiceResultData responseElement = await this.SendRegixRequest(
                    client,
                    serviceOP,
                    xDoc.DocumentElement!);

                if (responseElement.HasError)
                {
                    return new RegixPersonInfoResponse
                    {
                        ErrorMessage = responseElement.Error,
                        Success = false,
                        FirstName = string.Empty,
                        SurName = string.Empty,
                        FamilyName = string.Empty,
                        BirthDate = null
                    };
                }

                XmlElement xmlItem = responseElement.Data.Response.Any;

                return this.ParseXmlResponse(xmlItem);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }

            return null;
        }

        public RegixPersonInfoResponse ParseXmlResponse(XmlElement xmlItem)
        {
            TextInfo ti = CultureInfo.InvariantCulture.TextInfo;

            string firstName =
                ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("FirstName")!.Item(0)!.InnerText));

            string surName =
                ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("SurName")!.Item(0)!.InnerText));

            string familyName =
                ti.ToTitleCase(ti.ToLower(xmlItem.GetElementsByTagName("FamilyName")!.Item(0)!.InnerText));

            string dateOfBirthXml = xmlItem.GetElementsByTagName("BirthDate")!.Item(0)!.InnerText;

            DateTime.TryParseExact(
                dateOfBirthXml,
                RegixDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime birthDate);

            return new RegixPersonInfoResponse
            {
                ErrorMessage = null,
                Success = true,
                FirstName = firstName,
                SurName = surName,
                FamilyName = familyName,
                BirthDate = birthDate
            };
        }

        private async Task<ServiceResultData> SendRegixRequest(
            RegiXEntryPointClient client,
            string operation,
            XmlElement requestElement)
        {
            string certificateStore =
                this.options.Certificate.Store
                ?? throw new Exception("Missing RegixCertificateStore option");

            StoreLocation certificateStoreLocation =
                this.options.Certificate.StoreLocation
                ?? throw new Exception("Missing RegixCertificateStoreLocation option");

            string certificateName =
                this.options.Certificate.Name
                ?? throw new Exception("Missing RegixCertificateName option");

            using X509Certificate2? x509Certificate =
                  X509Certificate2Utils.LoadX509CertificateBySubjectName(
                      certificateStore,
                      certificateStoreLocation,
                      certificateName)
                  ?? throw new DomainException("Missing regix certificate");

            client.ClientCredentials.ClientCertificate.Certificate = x509Certificate;

            ServiceRequestData request = new();
            request.Operation = operation;
            request.CallContext = new CallContext
            {
                AdministrationName = this.options.Settings.AdministrationName,
                AdministrationOId = this.options.Settings.AdministrationOid,
                LawReason = this.options.Settings.LawReason,
                Remark = this.options.Settings.Remark,
                ResponsiblePersonIdentifier = this.options.Settings.ResponsiblePerson,
                ServiceType = this.options.Settings.ServiceType,
                ServiceURI = this.options.Settings.ServiceURI
            };

            request.SignResult = false;
            request.ReturnAccessMatrix = false;
            request.Argument = requestElement;
            ServiceResultData result = await client.ExecuteSynchronousAsync(request);

            return result;
        }

        public record RegixPersonInfoResponse
        {
            public string? ErrorMessage { get; set; }
            public bool Success { get; set; }
            public string? FirstName { get; set; }
            public string? SurName { get; set; }
            public string? FamilyName { get; set; }
            public DateTime? BirthDate { get; set; }
        }
    }
}
