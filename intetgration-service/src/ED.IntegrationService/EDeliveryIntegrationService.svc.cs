using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ED.Blobs;
using ED.DomainServices.IntegrationService;
using EDelivery.Common.DataContracts;
using EDelivery.Common.DataContracts.ESubject;
using EDelivery.Common.Enums;
using log4net;
using static ED.DomainServices.IntegrationService.IntegrationService;

namespace ED.IntegrationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple/* Namespace = "https://edelivery.egov.bg/services/integration", */)]
    public class EDeliveryIntegrationService : IEDeliveryIntegrationService
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

        private TimeSpan CacheExpiration { get; set; }

        public EDeliveryIntegrationService()
        {
            if (TimeSpan.TryParse(
               ConfigurationManager.AppSettings["CertificateValidatorCacheExpiration"],
               out TimeSpan cacheSlidingExpiration))
            {
                this.CacheExpiration = cacheSlidingExpiration;
            }
        }

        /// <summary>
        /// Get a list of all registered institutions
        /// </summary>
        /// <returns>list of all registered institutions</returns>
        public async Task<List<DcInstitutionInfo>> GetRegisteredInstitutions()
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        string.Empty);

                ED.DomainServices.IntegrationService.GetRegisteredInstitutionsResponse resp2 =
                    await client.GetRegisteredInstitutionsAsync(
                        new Google.Protobuf.WellKnownTypes.Empty());

                List<DcInstitutionInfo> result =
                    DataContractMapper.ToDcInstitutionInfo(resp2);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"Error occured in GetRegisteredInstitutions with params - certificate: {certificateThumbprint}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send an electronic document for delivery
        /// </summary>
        /// <param name="subject">sending subject</param>
        /// <param name="docBytes">document bytes</param>
        /// <param name="docNameWithExtension">document name with extension</param>
        /// <param name="docRegNumber">document registration number</param>
        /// <param name="receiverType">type of receiver - person/legal/institution</param>
        /// <param name="receiverUniqueIdentifier">egn for person, eik for legal, eik for institution</param>
        /// <param name="receiverPhone">receiver phone (if any)</param>
        /// <param name="receiverEmail">receiver email (if any)</param>
        /// <param name="operatorEGN">current sender EGN (if null or empty string passed - the integration login is used instead)</param>
        /// <returns>message id</returns>
        public async Task<int> SendElectronicDocument(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string receiverPhone,
            string receiverEmail,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (docBytes == null || docBytes.Length == 0)
                {
                    throw new ArgumentException("Document bytes must be provided");
                }

                if (string.IsNullOrEmpty(docNameWithExtension)
                    || string.IsNullOrEmpty(System.IO.Path.GetExtension(docNameWithExtension)))
                {
                    throw new ArgumentException("Document name with extension is missing or in invalid format");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                BlobsServiceClient.UploadBlobVO uploadedBlob =
                    await blobsServiceClient.UploadProfileBlobAsync(
                        docNameWithExtension,
                        docBytes.AsMemory(0, docBytes.Length),
                        authenticationInfo.ProfileId,
                        authenticationInfo.OperatorLoginId
                            ?? authenticationInfo.LoginId,
                        ProfileBlobAccessKeyType.Temporary,
                        docRegNumber,
                        CancellationToken.None);

                this.ValidateUploadedBlob(docNameWithExtension, uploadedBlob);

                UploadedBlobDO[] blobs = new UploadedBlobDO[]
                {
                    new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        docNameWithExtension,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(docBytes.Length))
                };

                ED.DomainServices.IntegrationService.SendMessage1Response resp2 =
                    await client.SendMessage1Async(
                        new ED.DomainServices.IntegrationService.SendMessage1Request
                        {
                            RecipientIdentifier = receiverUniqueIdentifier,
                            RecipientPhone = receiverPhone,
                            RecipientEmail = receiverEmail,
                            RecipientTargetGroupId = DataContractMapper.ToTargetGroupId(receiverType),
                            MessageSubject = subject,
                            MessageBody = string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1Request.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            SenderProfileId = authenticationInfo.ProfileId,
                            SenderLoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            ServiceOid = serviceOID,
                            SendEvent = "SendElectronicDocument",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendElectronicDocument with params -
    certificate: {certificateThumbprint},
    subject: {subject},
    docBytes: {docBytes?.Length},
    docNameWithExtension: {docNameWithExtension},
    docRegNumber: {docRegNumber},
    receiverType: {receiverType},
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    receiverPhone: {receiverPhone},
    receiverEmail: {receiverEmail},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send electronic document with access code
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="docBytes"></param>
        /// <param name="docNameWithExtension"></param>
        /// <param name="docRegNumber"></param>
        /// <param name="receiver"></param>
        /// <param name="serviceOID"></param>
        /// <param name="operatorEGN"></param>
        /// <returns></returns>
        public async Task<int> SendElectronicDocumentWithAccessCode(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            DcMessageWithCodeReceiver receiver,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (docBytes == null || docBytes.Length == 0)
                {
                    throw new ArgumentException("Document bytes must be provided");
                }

                if (string.IsNullOrEmpty(docNameWithExtension)
                    || string.IsNullOrEmpty(System.IO.Path.GetExtension(docNameWithExtension)))
                {
                    throw new ArgumentException(
                        "Document name with extension is missing or in invalid format");
                }

                if (receiver == null || string.IsNullOrEmpty(receiver?.EGNorLNCH))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (string.IsNullOrEmpty(receiver.FirstName)
                    || string.IsNullOrEmpty(receiver.LastName))
                {
                    throw new ArgumentException(
                        "Receiver's FirstName and LastName are required!");
                }

                if (string.IsNullOrEmpty(receiver.Email))
                {
                    throw new ArgumentException("Receiver's Email is required!");
                }

                if (string.IsNullOrEmpty(receiver.Phone))
                {
                    throw new ArgumentException("Receiver's Phone is required!");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                // TODO: cache?
                ED.DomainServices.IntegrationService.GetCodeSenderResponse resp =
                    await client.GetCodeSenderAsync(
                        new ED.DomainServices.IntegrationService.GetCodeSenderRequest
                        {
                            CertificateThumbprint = certificateThumbprint,
                            OperatorIdentifier = operatorEGN,
                        });

                if (resp.Sender == null)
                {
                    throw new UnauthorizedAccessException(
                        "Sender is not authorized to send messages with access code!");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                BlobsServiceClient.UploadBlobVO uploadedBlob =
                    await blobsServiceClient.UploadProfileBlobAsync(
                        docNameWithExtension,
                        docBytes.AsMemory(0, docBytes.Length),
                        resp.Sender.ProfileId,
                        resp.Sender.LoginId,
                        ProfileBlobAccessKeyType.Temporary,
                        docRegNumber,
                        CancellationToken.None);

                this.ValidateUploadedBlob(docNameWithExtension, uploadedBlob);

                UploadedBlobDO[] blobs = new UploadedBlobDO[]
                {
                    new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        docNameWithExtension,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(docBytes.Length))
                };

                ED.DomainServices.IntegrationService.SendMessage1WithAccessCodeResponse resp2 =
                await client.SendMessage1WithAccessCodeAsync(
                    new ED.DomainServices.IntegrationService.SendMessage1WithAccessCodeRequest
                    {
                        RecipientFirstName = receiver.FirstName,
                        RecipientMiddleName = receiver.MiddleName,
                        RecipientLastName = receiver.LastName,
                        RecipientIdentifier = receiver.EGNorLNCH,
                        RecipientPhone = receiver.Phone,
                        RecipientEmail = receiver.Email,
                        MessageSubject = subject,
                        MessageBody = string.Empty,
                        Blobs =
                        {
                            blobs.Select(x =>
                                new SendMessage1WithAccessCodeRequest.Types.Blob
                                {
                                    FileName = x.FileName,
                                    HashAlgorithm = x.HashAlgorithm,
                                    Hash = x.Hash,
                                    Size = x.Size,
                                    BlobId = x.BlobId,
                                })
                        },
                        ServiceOid = serviceOID,
                        SenderProfileId = resp.Sender.ProfileId,
                        SenderLoginId = resp.Sender.LoginId,
                        SendEvent = "SendElectronicDocumentWithAccessCode",
                    });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendElectronicDocumentWithAccessCode with params -
    certificate: {certificateThumbprint},
    subject: {subject},
    docBytes: {docBytes?.Length},
    docNameWithExtension: {docNameWithExtension},
    docRegNumber: {docRegNumber},
    receivedIdentifier: {receiver?.EGNorLNCH},
    receiverFirstName: {receiver?.FirstName},
    receiverMiddleName: {receiver?.MiddleName},
    receiverLastName: {receiver?.LastName},
    receiverPhone: {receiver?.Phone},
    receiverEmail: {receiver?.Email},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send an electronic document for delivery on behalf of another subject (from person to institutuion)
        /// (FOR BulSI needs)
        /// </summary>
        public async Task<int> SendElectronicDocumentOnBehalfOf(
            string subject,
            byte[] docBytes,
            string docNameWithExtension,
            string docRegNumber,
            eProfileType senderType,
            string senderUniqueIdentifier,
            string senderPhone,
            string senderEmail,
            string senderFirstName,
            string senderLastName,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (docBytes == null || docBytes.Length == 0)
                {
                    throw new ArgumentException("Document bytes must be provided");
                }

                if (string.IsNullOrEmpty(docNameWithExtension)
                    || string.IsNullOrEmpty(System.IO.Path.GetExtension(docNameWithExtension)))
                {
                    throw new ArgumentException(
                        "Document name with extension is missing or has invalid format");
                }

                if (string.IsNullOrEmpty(senderUniqueIdentifier))
                {
                    throw new ArgumentException("Sender is missing");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (receiverType != eProfileType.Institution)
                {
                    throw new ArgumentException(
                        "Receiver must be of type Institution. The method does not support sending to subject of another type!");
                }

                if (senderType != eProfileType.Person
                    && string.IsNullOrWhiteSpace(operatorEGN))
                {
                    throw new ArgumentException(
                        "Parameter operatorEGN is required in case when senderType is LegalPerson OR Institution! The operatorEGN should be the personalIdentifier of a person with active registration in the Edelivery System and with access to the sender's profile!");
                }

                string finalOperatorEGN = null;

                if (senderType != eProfileType.Person)
                {
                    finalOperatorEGN = operatorEGN;
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        null);

                CheckOperatorAccess checkOperatorAccess =
                    await this.CheckOperatorAccessAsync(
                        client,
                        certificateThumbprint,
                        senderUniqueIdentifier,
                        finalOperatorEGN);

                if (!checkOperatorAccess.HasAccess)
                {
                    throw new UnauthorizedAccessException("Unauthorized operator");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                BlobsServiceClient.UploadBlobVO uploadedBlob =
                    await blobsServiceClient.UploadProfileBlobAsync(
                        docNameWithExtension,
                        docBytes.AsMemory(0, docBytes.Length),
                        BlobsServiceClient.SystemProfileId,
                        checkOperatorAccess.OperatorLoginId
                            ?? authenticationInfo.LoginId,
                        ProfileBlobAccessKeyType.Temporary,
                        docRegNumber,
                        CancellationToken.None);

                this.ValidateUploadedBlob(docNameWithExtension, uploadedBlob);

                UploadedBlobDO[] blobs = new UploadedBlobDO[]
                {
                    new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        docNameWithExtension,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(docBytes.Length))
                };

                ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfResponse resp2 =
                    await client.SendMessage1OnBehalfOfAsync(
                        new ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfRequest
                        {
                            SenderIdentifier = senderUniqueIdentifier,
                            SenderPhone = senderPhone,
                            SenderEmail = senderEmail,
                            SenderFirstName = senderFirstName,
                            SenderLastName = senderLastName,
                            SenderTargetGroupId = DataContractMapper.ToTargetGroupId(senderType),
                            RecipientIdentifier = receiverUniqueIdentifier,
                            RecipientTargetGroupId = DataContractMapper.ToTargetGroupId(receiverType),
                            MessageSubject = subject,
                            MessageBody = string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1OnBehalfOfRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SentViaLoginId = authenticationInfo.LoginId,
                            SentViaOperatorLoginId = checkOperatorAccess.OperatorLoginId,
                            SendEvent = "SendMessageOnBehalfOf",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendElectronicDocumentOnBehalfOf with params -
    certificate: {certificateThumbprint},
    subject: {subject},
    docBytes: {docBytes?.Length},
    docNameWithExtension: {docNameWithExtension},
    docRegNumber: {docRegNumber},
    senderType: {senderType},
    senderUniqueIdentifier: {senderUniqueIdentifier},
    senderPhone: {senderPhone},
    senderEmail: {senderEmail},
    senderFirstName: {senderFirstName},
    senderLastName: {senderLastName},
    receiverType: {receiverType},
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send electronic message - the message can include more than one document
        /// </summary>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>message id</returns>
        public async Task<int> SendMessage(
            DcMessageDetails message,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string receiverPhone,
            string receiverEmail,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    throw new ArgumentException
                        ("Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            authenticationInfo.ProfileId,
                            authenticationInfo.OperatorLoginId
                                ?? authenticationInfo.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessage1Response resp2 =
                    await client.SendMessage1Async(
                        new ED.DomainServices.IntegrationService.SendMessage1Request
                        {
                            RecipientIdentifier = receiverUniqueIdentifier,
                            RecipientPhone = receiverPhone,
                            RecipientEmail = receiverEmail,
                            RecipientTargetGroupId = DataContractMapper.ToTargetGroupId(receiverType),
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1Request.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SenderProfileId = authenticationInfo.ProfileId,
                            SenderLoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            SendEvent = "SendMessage",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessage with params -
    certificate: {certificateThumbprint},
    messageTitle: {message?.Title}
    receiverType: {receiverType},
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    receiverPhone: {receiverPhone},
    receiverEmail: {receiverEmail},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        public async Task<int> SendMessageWithAccessCode(
            DcMessageDetails message,
            DcMessageWithCodeReceiver receiver,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    throw new ArgumentException(
                        "Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (receiver == null
                    || string.IsNullOrEmpty(receiver?.EGNorLNCH))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (string.IsNullOrEmpty(receiver.FirstName)
                    || string.IsNullOrEmpty(receiver.LastName))
                {
                    throw new ArgumentException(
                        "Receiver's FirstName and LastName are required!");
                }

                if (string.IsNullOrEmpty(receiver.Email))
                {
                    throw new ArgumentException("Receiver's Email is required!");
                }

                if (string.IsNullOrEmpty(receiver.Phone))
                {
                    throw new ArgumentException("Receiver's Phone is required!");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                // TODO: cache?
                ED.DomainServices.IntegrationService.GetCodeSenderResponse resp =
                    await client.GetCodeSenderAsync(
                        new ED.DomainServices.IntegrationService.GetCodeSenderRequest
                        {
                            CertificateThumbprint = certificateThumbprint,
                            OperatorIdentifier = operatorEGN,
                        });

                if (resp.Sender == null)
                {
                    throw new UnauthorizedAccessException(
                        "Sender is not authorized to send messages with access code!");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            resp.Sender.ProfileId,
                            resp.Sender.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessage1WithAccessCodeResponse resp2 =
                    await client.SendMessage1WithAccessCodeAsync(
                        new ED.DomainServices.IntegrationService.SendMessage1WithAccessCodeRequest
                        {
                            RecipientFirstName = receiver.FirstName,
                            RecipientMiddleName = receiver.MiddleName,
                            RecipientLastName = receiver.LastName,
                            RecipientIdentifier = receiver.EGNorLNCH,
                            RecipientPhone = receiver.Phone,
                            RecipientEmail = receiver.Email,
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1WithAccessCodeRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SenderProfileId = resp.Sender.ProfileId,
                            SenderLoginId = resp.Sender.LoginId,
                            SendEvent = "SendMessageWithAccessCode",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessageWithAccessCode with params -
    certificate: {certificateThumbprint},
    messageTitle: {message?.Title},
    receivedIdentifier: {receiver?.EGNorLNCH},
    receiverFirstName: {receiver?.FirstName},
    receiverMiddleName: {receiver?.MiddleName},
    receiverLastName: {receiver?.LastName},
    receiverPhone: {receiver?.Phone},
    receiverEmail: {receiver?.Email},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send electronic message in reply to a received message
        /// </summary>
        /// <param name="message">The messaage to send</param>
        /// <param name="replyToMessageId">the id of the message the reply is made to</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<int> SendMessageInReplyTo(
            DcMessageDetails message,
            int replyToMessageId,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    logger.Error("Error in SendMessageInReplyTo - Message does not have all required fields: Title, Text or at least one Attached Document");
                    throw new ArgumentException("Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (replyToMessageId == 0)
                {
                    logger.Error("Error in SendMessageInReplyTo - Mandatory parameter replyToMessageId is missing or is equal to zero!");
                    throw new ArgumentException("Mandatory parameter replyToMessageId is missing or is equal to zero!");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            authenticationInfo.ProfileId,
                            authenticationInfo.OperatorLoginId
                                ?? authenticationInfo.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessageInReplyToResponse resp2 =
                    await client.SendMessageInReplyToAsync(
                        new ED.DomainServices.IntegrationService.SendMessageInReplyToRequest
                        {
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessageInReplyToRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ReplyToMessageId = replyToMessageId,
                            ServiceOid = serviceOID,
                            SenderProfileId = authenticationInfo.ProfileId,
                            SenderLoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            SendEvent = "SendMessageInReplyTo",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessageInReplyTo with params -
    certificate: {certificateThumbprint},
    messageTitle: {message?.Title},
    replyToMessageId: {replyToMessageId},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Send electronic message on behalf of (from person/legal to institutuion) - the message can include more than one document
        /// </summary>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>message id</returns>
        public async Task<int> SendMessageOnBehalfOf(
            DcMessageDetails message,
            eProfileType senderType,
            string senderUniqueIdentifier,
            string senderPhone,
            string senderEmail,
            string senderFirstName,
            string senderLastName,
            eProfileType receiverType,
            string receiverUniqueIdentifier,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    throw new ArgumentException(
                        "Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (string.IsNullOrEmpty(senderUniqueIdentifier))
                {
                    throw new ArgumentException("Sender is missing");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (receiverType != eProfileType.Institution)
                {
                    throw new ArgumentException(
                        "Receiver must be of type Institution. The method does not support sending to subject of another type!");
                }

                if (senderType != eProfileType.Person
                    && string.IsNullOrWhiteSpace(operatorEGN))
                {
                    throw new ArgumentException(
                        "Parameter operatorEGN is required in case when senderType is LegalPerson OR Institution! The operatorEGN should be the personalIdentifier of a person with active registration in the Edelivery System and with access to the sender's profile!");
                }

                string finalOperatorEGN = null;

                if (senderType != eProfileType.Person)
                {
                    finalOperatorEGN = operatorEGN;
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        null);

                CheckOperatorAccess checkOperatorAccess =
                   await this.CheckOperatorAccessAsync(
                       client,
                       certificateThumbprint,
                       senderUniqueIdentifier,
                       finalOperatorEGN);

                if (!checkOperatorAccess.HasAccess)
                {
                    throw new UnauthorizedAccessException("Unauthorized operator");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            BlobsServiceClient.SystemProfileId,
                            checkOperatorAccess.OperatorLoginId
                                ?? authenticationInfo.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfResponse resp2 =
                    await client.SendMessage1OnBehalfOfAsync(
                        new ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfRequest
                        {
                            SenderIdentifier = senderUniqueIdentifier,
                            SenderPhone = senderPhone,
                            SenderEmail = senderEmail,
                            SenderFirstName = senderFirstName,
                            SenderLastName = senderLastName,
                            SenderTargetGroupId = DataContractMapper.ToTargetGroupId(senderType),
                            RecipientIdentifier = receiverUniqueIdentifier,
                            RecipientTargetGroupId = DataContractMapper.ToTargetGroupId(receiverType),
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1OnBehalfOfRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SentViaLoginId = authenticationInfo.LoginId,
                            SentViaOperatorLoginId = checkOperatorAccess.OperatorLoginId,
                            SendEvent = "SendMessageOnBehalfOf",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessageOnBehalfOf with params -
    certificate: {certificateThumbprint},
    messageTitle: {message?.Title},
    senderType: {senderType},
    senderUniqueIdentifier: {senderUniqueIdentifier}, 
    senderPhone: {senderPhone},
    senderEmail: {senderEmail},
    senderFirstName: {senderFirstName},
    senderLastName: {senderLastName},
    receiverType: {receiverType}, 
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get sent document 
        /// </summary>
        /// <param name="documentRegistrationNumber">document reg number</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>sent message without document(s) content</returns>
        public async Task<DcMessageDetails> GetSentDocumentStatusByRegNum(
            string documentRegistrationNumber,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentDocumentStatusByRegNumResponse resp2 =
                    await client.GetSentDocumentStatusByRegNumAsync(
                        new ED.DomainServices.IntegrationService.GetSentDocumentStatusByRegNumRequest
                        {
                            DocumentRegistrationNumber = documentRegistrationNumber,
                            ProfileId = authenticationInfo.ProfileId,
                            LoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            OpenEvent = "GetSentDocumentStatusByRegNum",
                        });

                return DataContractMapper.ToDcMessageDetails(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentDocumentStatusByRegNum with params -
    certificate: {certificateThumbprint},
    documentRegistrationNumber: {documentRegistrationNumber},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get sent document message with status and timestamp, withoud document content
        /// </summary>
        /// <param name="messageId">message id</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>the message without the document content</returns>
        public async Task<DcMessageDetails> GetSentMessageStatus(
            int messageId,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentMessageStatusResponse resp2 =
                    await client.GetSentMessageStatusAsync(
                        new ED.DomainServices.IntegrationService.GetSentMessageStatusRequest
                        {
                            MessageId = messageId,
                            ProfileId = authenticationInfo.ProfileId,
                            LoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            OpenEvent = "GetSentMessageStatus",
                        });

                return DataContractMapper.ToDcMessageDetails(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentMessageStatus with params -
    certificate: {certificateThumbprint},
    messageId: {messageId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get electronic subject info
        /// </summary>
        /// <param name="receivedMessageId"></param>
        /// <returns></returns>
        public async Task<DcSubjectInfo> GetSubjectInfo(
            Guid electronicSubjectId,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetProfileInfoResponse resp2 =
                    await client.GetProfileInfoAsync(
                        new ED.DomainServices.IntegrationService.GetProfileInfoRequest
                        {
                            ProfileSubjectId = electronicSubjectId.ToString(),
                        });

                return DataContractMapper.ToDcSubjectInfo(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSubjectInfo with params -
    certificate: {certificateThumbprint},
    electronicSubjectId: {electronicSubjectId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get sent document content and timestamp by registration number
        /// </summary>
        /// <param name="documentRegistrationNumber"></param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<DcDocument> GetSentDocumentContentByRegNum(
            string documentRegistrationNumber,
            string operatorEGN)
        {
            // TODO: test if recipient can use this method
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentDocumentContentByRegNumResponse resp2 =
                    await client.GetSentDocumentContentByRegNumAsync(
                        new ED.DomainServices.IntegrationService.GetSentDocumentContentByRegNumRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            DocumentRegistrationNumber = documentRegistrationNumber,
                        });

                var result = DataContractMapper.ToDcDocument(resp2);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                BlobsServiceClient.DownloadBlobToArrayVO blob =
                    await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                        authenticationInfo.ProfileId,
                        result.Id,
                        resp2.Blob.MessageId,
                        default);

                result.Content = blob.Content;

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentDocumentContentByRegNum with params -
    certificate: {certificateThumbprint},
    documentRegistrationNumber: {documentRegistrationNumber},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get sent documents in a message
        /// </summary>
        /// <param name="messageId">message id</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>document content, name, regnumber and timestamp</returns>
        public async Task<List<DcDocument>> GetSentDocumentsContent(
            int messageId,
            string operatorEGN)
        {
            // TODO: test if recipient can use this method
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentDocumentsContentResponse resp2 =
                    await client.GetSentDocumentsContentAsync(
                        new ED.DomainServices.IntegrationService.GetSentDocumentsContentRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            MessageId = messageId,
                        });

                var result = DataContractMapper.ToListOfDcDocument(resp2);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                foreach (var document in result)
                {
                    BlobsServiceClient.DownloadBlobToArrayVO blob =
                        await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                            authenticationInfo.ProfileId,
                            document.Id,
                            messageId,
                            default);

                    document.Content = blob.Content;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentDocumentsContent with params -
    certificate: {certificateThumbprint},
    messageId: {messageId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get sent document by id
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns>document content, name, regnumber and timestamp</returns>
        public async Task<DcDocument> GetSentDocumentContent(
            int documentId,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentDocumentContentResponse resp2 =
                   await client.GetSentDocumentContentAsync(
                       new ED.DomainServices.IntegrationService.GetSentDocumentContentRequest
                       {
                           ProfileId = authenticationInfo.ProfileId,
                           BlobId = documentId,
                       });

                var result = DataContractMapper.ToDcDocument(resp2);

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                BlobsServiceClient.DownloadBlobToArrayVO blob =
                    await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                        authenticationInfo.ProfileId,
                        result.Id,
                        resp2.Blob.MessageId,
                        default);

                result.Content = blob.Content;

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentDocumentContent with params -
    certificate: {certificateThumbprint},
    documentId: {documentId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a list of all sent messages
        /// </summary>
        /// <param name="operatorEGN">if provided - the system checks wether it is a login and has rights to that profile</param>
        /// <returns>list of messages</returns>
        public async Task<List<DcMessage>> GetSentMessagesList(string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.OutboxResponse resp =
                    await client.OutboxAsync(
                        new ED.DomainServices.IntegrationService.OutboxRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            Offset = 0,
                            Limit = 100,
                        });

                return DataContractMapper.ToListOfDcMessage(resp);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentMessagesList with params -
    certificate: {certificateThumbprint},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a list of sent messages per pages
        /// </summary>
        /// <param name="operatorEGN">if provided - the system checks wether it is a login and has rights to that profile</param>
        /// <returns>paged list of messages</returns>
        public async Task<DcPartialList<DcMessage>> GetSentMessagesListPaged(
            int pageNumber,
            int pageSize,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                int limit = Math.Max(1, pageSize);
                int offset = (Math.Max(1, pageNumber) - 1) * limit;

                ED.DomainServices.IntegrationService.OutboxResponse resp =
                    await client.OutboxAsync(
                        new ED.DomainServices.IntegrationService.OutboxRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            Offset = offset,
                            Limit = limit,
                        });

                return new DcPartialList<DcMessage>(
                    resp.Length,
                    DataContractMapper.ToListOfDcMessage(resp));
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentMessagesListPaged with params -
    certificate: {certificateThumbprint},
    pageNumber: {pageNumber},
    pageSize: {pageSize},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a list or received messages
        /// </summary>
        /// <param name="onlyNew">all messages or only new</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<List<DcMessage>> GetReceivedMessagesList(
            bool onlyNew,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.InboxResponse resp =
                    await client.InboxAsync(
                        new ED.DomainServices.IntegrationService.InboxRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            ShowOnlyNew = onlyNew,
                            Offset = 0,
                            Limit = 100,
                        });

                return DataContractMapper.ToListOfDcMessage(resp);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetReceivedMessagesList with params -
    certificate: {certificateThumbprint},
    onlyNew: {onlyNew},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a paged list of received messages
        /// </summary>
        /// <param name="onlyNew">all messages or only new ones</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="pageSize">page size</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<DcPartialList<DcMessage>> GetReceivedMessagesListPaged(
            bool onlyNew,
            int pageNumber,
            int pageSize,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                int limit = Math.Max(1, pageSize);
                int offset = (Math.Max(1, pageNumber) - 1) * limit;

                ED.DomainServices.IntegrationService.InboxResponse resp =
                    await client.InboxAsync(
                        new ED.DomainServices.IntegrationService.InboxRequest
                        {
                            ProfileId = authenticationInfo.ProfileId,
                            ShowOnlyNew = onlyNew,
                            Offset = offset,
                            Limit = limit,
                        });

                return new DcPartialList<DcMessage>(
                    resp.Length,
                    DataContractMapper.ToListOfDcMessage(resp));
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetReceivedMessagesListPaged with params -
    certificate: {certificateThumbprint},
    onlyNew: {onlyNew},
    pageNumber: {pageNumber},
    pageSize: {pageSize},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a received message
        /// </summary>
        /// <param name="messageId">The id of the message</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<DcMessageDetails> GetReceivedMessageContent(
            int messageId,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetReceivedMessageContentResponse resp2 =
                    await client.GetReceivedMessageContentAsync(
                        new ED.DomainServices.IntegrationService.GetReceivedMessageContentRequest
                        {
                            MessageId = messageId,
                            ProfileId = authenticationInfo.ProfileId,
                            LoginId =
                                authenticationInfo.OperatorLoginId
                                    ?? authenticationInfo.LoginId,
                            OpenEvent = "GetReceivedMessageContent",
                        });

                var result = DataContractMapper.ToDcMessageDetails(resp2);

                int messageBlobId = resp2.Message.ForwardedMessage != null
                    ? resp2.Message.ForwardedMessage.MessageId
                    : resp2.Message.MessageId;

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                foreach (var attachment in result.AttachedDocuments)
                {
                    BlobsServiceClient.DownloadBlobToArrayVO blob =
                        await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                            authenticationInfo.ProfileId,
                            attachment.Id,
                            messageBlobId,
                            default);

                    attachment.Content = blob.Content;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetReceivedMessageContent with params -
    certificate: {certificateThumbprint},
    messageId: {messageId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get a sent message
        /// </summary>
        /// <param name="messageId">The id of the message</param>
        /// <param name="operatorEGN">operatorId - if provided, check for existence and rights! if not provided - pass the default integration login</param>
        /// <returns></returns>
        public async Task<DcMessageDetails> GetSentMessageContent(
            int messageId,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        operatorEGN);

                ED.DomainServices.IntegrationService.GetSentMessageContentResponse resp2 =
                    await client.GetSentMessageContentAsync(
                        new ED.DomainServices.IntegrationService.GetSentMessageContentRequest
                        {
                            MessageId = messageId,
                            ProfileId = authenticationInfo.ProfileId,
                        });

                var result = DataContractMapper.ToDcMessageDetails(resp2);

                int messageBlobId = resp2.Message.ForwardedMessage != null
                    ? resp2.Message.ForwardedMessage.MessageId
                    : resp2.Message.MessageId;

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                foreach (var attachment in result.AttachedDocuments)
                {
                    BlobsServiceClient.DownloadBlobToArrayVO blob =
                        await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                            authenticationInfo.ProfileId,
                            attachment.Id,
                            messageBlobId,
                            default);

                    attachment.Content = blob.Content;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in GetSentMessageContent with params -
    certificate: {certificateThumbprint},
    messageId: {messageId},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Check if a person has registration and if yes -> return profiles he/she has access to (name, EIK)
        /// </summary>
        /// <param name="personIdentificator">EGN or LNCh</param>
        /// <returns></returns>
        public async Task<DcPersonRegistrationInfo> CheckPersonHasRegistration(
            string personIdentificator)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (string.IsNullOrEmpty(personIdentificator))
                {
                    return new DcPersonRegistrationInfo(personIdentificator);
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        string.Empty);

                ED.DomainServices.IntegrationService.CheckIndividualRegistrationResponse resp2 =
                    await client.CheckIndividualRegistrationAsync(
                        new ED.DomainServices.IntegrationService.CheckIndividualRegistrationRequest
                        {
                            Identifier = personIdentificator
                        });

                return DataContractMapper.ToDcPersonRegistrationInfo(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in CheckPersonHasRegistration with params -
    certificate: {certificateThumbprint},
    personIdentificator: {personIdentificator}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Check if a legal person has registration and if yes -> return name, EIK, phone, email
        /// </summary>
        /// <param name="personIdentificator">EGN or LNCh</param>
        /// <returns></returns>
        public async Task<DcLegalPersonRegistrationInfo> CheckLegalPersonHasRegistration(
            string eik)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (string.IsNullOrEmpty(eik))
                {
                    return new DcLegalPersonRegistrationInfo(eik);
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        string.Empty);

                ED.DomainServices.IntegrationService.CheckLegalEntityRegistrationResponse resp2 =
                    await client.CheckLegalEntityRegistrationAsync(
                        new ED.DomainServices.IntegrationService.CheckLegalEntityRegistrationRequest
                        {
                            Identifier = eik,
                        });

                return DataContractMapper.ToDcLegalPersonRegistrationInfo(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in CheckLegalPersonHasRegistration with params -
    certificate: {certificateThumbprint},
    eik: {eik}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Check if subject has registration by his identificator
        ///  </summary>
        /// <param name="identificator">EGN,LNCH,EIK</param>
        /// <returns></returns>
        public async Task<DcSubjectRegistrationInfo> CheckSubjectHasRegistration(
            string identificator)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (string.IsNullOrEmpty(identificator))
                {
                    return new DcSubjectRegistrationInfo(identificator);
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        string.Empty);

                ED.DomainServices.IntegrationService.CheckProfileRegistrationResponse resp2 =
                   await client.CheckProfileRegistrationAsync(
                       new ED.DomainServices.IntegrationService.CheckProfileRegistrationRequest
                       {
                           Identifier = identificator,
                       });

                return DataContractMapper.ToDcSubjectRegistrationInfo(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in CheckSubjectHasRegistration with params -
    certificate: {certificateThumbprint},
    identificator: {identificator}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Get general statistics data
        /// </summary>
        /// <param name="startDate">Start date of the period to get statitics for</param>
        /// <returns></returns>
        public async Task<DcStatisticsGeneral> GetEDeliveryGeneralStatistics()
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        string.Empty);

                ED.DomainServices.IntegrationService.GetStatisticsResponse resp2 =
                    await client.GetStatisticsAsync(
                        new Google.Protobuf.WellKnownTypes.Empty());

                return DataContractMapper.ToDcStatisticsGeneral(resp2);
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"Error occured in GetEDeliveryGeneralStatistics with params - certificate: {certificateThumbprint}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        public async Task<int> SendMessageOnBehalfToPerson(
            DcMessageDetails message,
            string senderUniqueIdentifier,
            string receiverUniqueIdentifier,
            string receiverPhone,
            string receiverEmail,
            string receiverFirstName,
            string receiverLastName,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    throw new ArgumentException(
                        "Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (string.IsNullOrEmpty(senderUniqueIdentifier))
                {
                    throw new ArgumentException("Sender is missing");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (string.IsNullOrWhiteSpace(operatorEGN))
                {
                    throw new ArgumentException(
                        "Parameter operatorEGN is required! The operatorEGN should be the personalIdentifier of a person with active registration in the Edelivery System and with access to the sender's profile!");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        null);

                CheckOperatorAccess checkOperatorAccess =
                   await this.CheckOperatorAccessAsync(
                       client,
                       certificateThumbprint,
                       senderUniqueIdentifier,
                       operatorEGN);

                if (!checkOperatorAccess.HasAccess)
                {
                    throw new UnauthorizedAccessException("Unauthorized operator");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            BlobsServiceClient.SystemProfileId,
                            checkOperatorAccess.OperatorLoginId
                                ?? authenticationInfo.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfToIndividualResponse resp2 =
                    await client.SendMessage1OnBehalfOfToIndividualAsync(
                        new ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfToIndividualRequest
                        {
                            SenderIdentifier = senderUniqueIdentifier,
                            RecipientIdentifier = receiverUniqueIdentifier,
                            RecipientPhone = receiverPhone,
                            RecipientEmail = receiverEmail,
                            RecipientFirstName = receiverFirstName,
                            RecipientLastName = receiverLastName,
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1OnBehalfOfToIndividualRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SentViaLoginId = authenticationInfo.LoginId,
                            SentViaOperatorLoginId = checkOperatorAccess.OperatorLoginId,
                            SendEvent = "SendMessageOnBehalfToPerson",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessageOnBehalfToPerson with params -
    certificate: {certificateThumbprint},
    senderUniqueIdentifier: {senderUniqueIdentifier},
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    receiverPhone: {receiverPhone},
    receiverEmail: {receiverEmail},
    receiverFirstName: {receiverFirstName},
    receiverLastName: {receiverLastName},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        public async Task<int> SendMessageOnBehalfToLegalEntity(
            DcMessageDetails message,
            string senderUniqueIdentifier,
            string receiverUniqueIdentifier,
            string serviceOID,
            string operatorEGN)
        {
            string certificateThumbprint = string.Empty;

            try
            {
                certificateThumbprint = this.GetCertificateThumbprint();

                if (message == null
                    || string.IsNullOrEmpty(message.Title)
                    || (string.IsNullOrEmpty(message.MessageText)
                        && (message.AttachedDocuments == null
                            || message.AttachedDocuments.Count == 0)))
                {
                    throw new ArgumentException(
                        "Message does not have all required fields: Title, Text or at least one Attached Document");
                }

                if (string.IsNullOrEmpty(senderUniqueIdentifier))
                {
                    throw new ArgumentException("Sender is missing");
                }

                if (string.IsNullOrEmpty(receiverUniqueIdentifier))
                {
                    throw new ArgumentException("Receiver is missing");
                }

                if (string.IsNullOrWhiteSpace(operatorEGN))
                {
                    throw new ArgumentException(
                        "Parameter operatorEGN is required! The operatorEGN should be the personalIdentifier of a person with active registration in the Edelivery System and with access to the sender's profile!");
                }

                var client = GrpcClientFactory.CreateIntegrationServiceClient();

                AuthenticationInfo authenticationInfo =
                    await this.GetAuthenticationInfoAsync(
                        client,
                        certificateThumbprint,
                        null);

                CheckOperatorAccess checkOperatorAccess =
                   await this.CheckOperatorAccessAsync(
                       client,
                       certificateThumbprint,
                       senderUniqueIdentifier,
                       operatorEGN);

                if (!checkOperatorAccess.HasAccess)
                {
                    throw new UnauthorizedAccessException("Unauthorized operator");
                }

                BlobsServiceClient blobsServiceClient = new BlobsServiceClient(
                    GrpcClientFactory.CreateBlobsClient());

                int attachmentsCount = message.AttachedDocuments?.Count ?? 0;

                UploadedBlobDO[] blobs =
                    new UploadedBlobDO[attachmentsCount];

                for (int i = 0; i < attachmentsCount; i++)
                {
                    BlobsServiceClient.UploadBlobVO uploadedBlob =
                        await blobsServiceClient.UploadProfileBlobAsync(
                            message.AttachedDocuments[i].DocumentName,
                            message.AttachedDocuments[i].Content.AsMemory(0, message.AttachedDocuments[i].Content.Length),
                            BlobsServiceClient.SystemProfileId,
                            checkOperatorAccess.OperatorLoginId
                                ?? authenticationInfo.LoginId,
                            ProfileBlobAccessKeyType.Temporary,
                            message.AttachedDocuments[i].DocumentRegistrationNumber,
                            CancellationToken.None);

                    this.ValidateUploadedBlob(
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob);

                    blobs[i] = new UploadedBlobDO(
                        uploadedBlob.BlobId.Value,
                        message.AttachedDocuments[i].DocumentName,
                        uploadedBlob.HashAlgorithm,
                        uploadedBlob.Hash,
                        Convert.ToUInt64(message.AttachedDocuments[i].Content.Length));
                }

                ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfToLegalEntityResponse resp2 =
                    await client.SendMessage1OnBehalfOfToLegalEntityAsync(
                        new ED.DomainServices.IntegrationService.SendMessage1OnBehalfOfToLegalEntityRequest
                        {
                            SenderIdentifier = senderUniqueIdentifier,
                            RecipientIdentifier = receiverUniqueIdentifier,
                            MessageSubject = message.Title,
                            MessageBody = message.MessageText ?? string.Empty,
                            Blobs =
                            {
                                blobs.Select(x =>
                                    new SendMessage1OnBehalfOfToLegalEntityRequest.Types.Blob
                                    {
                                        FileName = x.FileName,
                                        HashAlgorithm = x.HashAlgorithm,
                                        Hash = x.Hash,
                                        Size = x.Size,
                                        BlobId = x.BlobId,
                                    })
                            },
                            ServiceOid = serviceOID,
                            SentViaLoginId = authenticationInfo.LoginId,
                            SentViaOperatorLoginId = checkOperatorAccess.OperatorLoginId,
                            SendEvent = "SendMessageOnBehalfToLegalEntity",
                        });

                if (resp2.IsSuccessful)
                {
                    return resp2.MessageId.Value;
                }
                else
                {
                    throw new Exception(resp2.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(
$@"Error occured in SendMessageOnBehalfToLegalEntity with params -
    certificate: {certificateThumbprint},
    senderUniqueIdentifier: {senderUniqueIdentifier},
    receiverUniqueIdentifier: {receiverUniqueIdentifier},
    serviceOID: {serviceOID},
    operatorEGN: {operatorEGN}",
                    ex);

                throw new FaultException(ex.Message);
            }
        }

        private string[] GetSpecialCertificateThumbprints()
        {
            string[] certs = ConfigurationManager.AppSettings["SpecialCertificateThumbprints"]
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return certs;
        }

        private async Task<AuthenticationInfo> GetAuthenticationInfoAsync(
            IntegrationServiceClient client,
            string certificateThumbprint,
            string operatorEGN)
        {
            AuthenticationInfo authenticationInfo =
                await MemoryCacheExtensions.CertificateDataCache.AddOrGetExistingAsync(
                    $"{certificateThumbprint}-{operatorEGN}",
                    async () =>
                    {
                        string[] specialCertificateThumbprints =
                            this.GetSpecialCertificateThumbprints();

                        bool isCurrentCertificateThumbprintSpecial =
                            specialCertificateThumbprints
                                .Contains(certificateThumbprint);

                        ED.DomainServices.IntegrationService.GetAuthenticationInfoResponse authResp =
                            await client.GetAuthenticationInfoAsync(
                                new ED.DomainServices.IntegrationService.GetAuthenticationInfoRequest
                                {
                                    CertificateThumbprint = certificateThumbprint,
                                    OperatorIdentifier = isCurrentCertificateThumbprintSpecial ? null : operatorEGN,
                                });

                        return authResp.AuthenticatedProfile != null
                            ? new AuthenticationInfo(
                                authResp.AuthenticatedProfile.ProfileId,
                                authResp.AuthenticatedProfile.LoginId,
                                authResp.AuthenticatedProfile.OperatorLoginId)
                            : null;
                    },
                    new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.Add(this.CacheExpiration)
                    }) 
                    ?? throw new UnauthorizedAccessException();

            return authenticationInfo;
        }

        private async Task<CheckOperatorAccess> CheckOperatorAccessAsync(
            IntegrationServiceClient client,
            string certificateThumbprint,
            string profileIdentifier,
            string operatorIdentifier)
        {
            string[] specialCertificateThumbprints =
                           this.GetSpecialCertificateThumbprints();

            bool isCurrentCertificateThumbprintSpecial =
                specialCertificateThumbprints
                    .Contains(certificateThumbprint);

            CheckProfileOperatorAccessResponse resp =
                await client.CheckProfileOperatorAccessAsync(
                    new CheckProfileOperatorAccessRequest
                    {
                        ProfileIdentifier = profileIdentifier,
                        OperatorIdentifier = isCurrentCertificateThumbprintSpecial ? null : operatorIdentifier,
                    });

            return new CheckOperatorAccess(resp);
        }

        private string GetCertificateThumbprint()
        {
            ClaimsPrincipal principal = ClaimsPrincipal.Current;

            if (principal == null
                || !principal.Identity.IsAuthenticated
                || !principal.HasClaim(p => p.Type == System
                .IdentityModel.Claims.ClaimTypes.Thumbprint))
            {
                throw new UnauthorizedAccessException("Invalid client certificate");
            }

            string claimThumbprint = principal
                .Claims
                .Single(x => x.Type == System
                .IdentityModel.Claims.ClaimTypes.Thumbprint)
                .Value;

            string certificateThumbprint =
                this.FromBase64ToHex(claimThumbprint);

            return certificateThumbprint;
        }

        private string FromBase64ToHex(string base64string)
        {
            if (string.IsNullOrEmpty(base64string))
            {
                return base64string;
            }

            byte[] bytes = Convert.FromBase64String(base64string);
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private void ValidateUploadedBlob(
            string fileName,
            BlobsServiceClient.UploadBlobVO blob)
        {
            if (blob.IsMalicious)
            {
                throw new Exception($"File {fileName} is malicious");
            }

            if (!blob.BlobId.HasValue
                || string.IsNullOrEmpty(blob.Hash)
                || string.IsNullOrEmpty(blob.HashAlgorithm))
            {
                throw new Exception($"File {fileName} could not be uploaded");
            }
        }
    }
}
