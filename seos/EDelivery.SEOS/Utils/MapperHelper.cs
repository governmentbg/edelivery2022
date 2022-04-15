using System;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.Models;
using AutoMapper;

namespace EDelivery.SEOS.Utils
{
    public class MapperHelper
    {
        public static Mapper Mapping { get; private set; }

        public static void Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SEOSMessageAttachment, AttachmentFileType>()
                .ForMember(dest => dest.AttBody, act => act.MapFrom(src => src.Content))
                .ForMember(dest => dest.AttComment, act => act.MapFrom(src => src.Comment))
                .ForMember(dest => dest.AttFileName, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.AttMIMEType, act => act.MapFrom(src => src.MimeType));

                cfg.CreateMap<SEOSMessageCorespondent, CorrespondentType>()
                .ForMember(dest => dest.CorAddress, act => act.MapFrom(src => src.Address))
                .ForMember(dest => dest.CorBULSTAT, act => act.MapFrom(src => src.Bulstat))
                .ForMember(dest => dest.CorCity, act => act.MapFrom(src => src.City))
                .ForMember(dest => dest.CorEGN, act => act.MapFrom(src => src.EGN))
                .ForMember(dest => dest.CorEMail, act => act.MapFrom(src => src.Email))
                .ForMember(dest => dest.CorIDCard, act => act.MapFrom(src => src.IDCard))
                .ForMember(dest => dest.CorMobilePhone, act => act.MapFrom(src => src.MobilePhone))
                .ForMember(dest => dest.CorMOL, act => act.MapFrom(src => src.MOL))
                .ForMember(dest => dest.CorName, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.CorPhone, act => act.MapFrom(src => src.Phone))
                .ForMember(dest => dest.CorKind, act => act.MapFrom(src => CorrespondentKindType.Corr_Other))
                .ForMember(dest => dest.CorKindSpecified, act => act.MapFrom(src => true));

                cfg.CreateMap<SEOSMessage, MessageResponse>()
                .ForMember(dest => dest.UniqueIdentifier, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => (DocumentStatusType)src.Status))
                .ForMember(dest => dest.RegIndex, act => act.MapFrom(src => src.DocNumberInternal))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src => src.Receiver.Name))
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => src.ReceiverElectronicSubjectId))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => src.Sender.Name))
                .ForMember(dest => dest.SenderId, act => act.MapFrom(src => src.SenderElectronicSubjectId))
                .ForMember(dest => dest.Subject, act => act.MapFrom(src => src.DocAbout))
                .ForMember(dest => dest.DateReceived, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.DateSent, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.DocumentKind, act => act.MapFrom(src => src.DocKind))
                .ForMember(dest => dest.DocumentReferenceNumber, act => act.MapFrom(src => src.DocReferenceNumber));

                cfg.CreateMap<SEOSMessage, MessageResponse>()
                .ForMember(dest => dest.UniqueIdentifier, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => (DocumentStatusType)src.Status))
                .ForMember(dest => dest.RegIndex, act => act.MapFrom(src => src.DocNumberInternal))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src => src.Receiver.Name))
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => src.ReceiverElectronicSubjectId))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => src.Sender.Name))
                .ForMember(dest => dest.SenderId, act => act.MapFrom(src => src.SenderElectronicSubjectId))
                .ForMember(dest => dest.Subject, act => act.MapFrom(src => src.DocAbout))
                .ForMember(dest => dest.DateReceived, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.DateSent, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.DocumentKind, act => act.MapFrom(src => src.DocKind))
                .ForMember(dest => dest.DocumentReferenceNumber, act => act.MapFrom(src => src.DocReferenceNumber));

                cfg.CreateMap<MessagesPage, MessagesPageResponse>()
                .ForMember(dest => dest.CountAllMessages, act => act.MapFrom(src => src.CountAllMessages))
                .ForMember(dest => dest.Messages, act => act.MapFrom(src => src.Messages));

                cfg.CreateMap<SEOSMessageAttachment, AttachmentResponse>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.MessageId, act => act.MapFrom(src => src.MessageId))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.Content, act => act.MapFrom(src => src.Content))
                .ForMember(dest => dest.MimeType, act => act.MapFrom(src => src.MimeType))
                .ForMember(dest => dest.Comment, act => act.MapFrom(src => src.Comment))
                .ForMember(dest => dest.ReceiverElectronicSubjectId, act => act.MapFrom(src =>
                src.Message != null
                ? src.Message.ReceiverElectronicSubjectId
                : null))
                .ForMember(dest => dest.SenderElectronicSubjectId, act => act.MapFrom(src =>
                src.Message != null
                ? src.Message.SenderElectronicSubjectId
                : null))
                .ForMember(dest => dest.MalwareScanResultId, act => act.MapFrom(src => src.MalwareScanResultId))
                .ForMember(dest => dest.MalwareScanResult, act => act.MapFrom(src =>
                src.MalwareScanResult != null
                ? new MalwareScanResultResponse
                {
                    DbItemId = src.Id,
                    FileName = src.Name,
                    IsMalicious = src.MalwareScanResult.IsMalicious.HasValue
                     ? src.MalwareScanResult.IsMalicious.Value
                     : false,
                    IsSuccessfulScan = true,
                    Message = src.MalwareScanResult.Message,
                    ErrorReason = MalwareScanErrorReasonEnum.NoError,
                    Status = src.MalwareScanResult.Status,
                    StatusDate = src.MalwareScanResult.StatusDate,
                    ElapsedTimeSeconds = src.MalwareScanResult.ElapsedTimeSeconds
                }
                : null));

                cfg.CreateMap<SEOSMessageCorespondent, CorespondentResponse>();

                cfg.CreateMap<SEOSMessage, OpenDocumentResponse>()
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.IsReceived, act => act.Ignore())
                .ForMember(dest => dest.Attachments, act => act.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.Corespondents, act => act.MapFrom(src => src.Corespondents))
                .ForMember(dest => dest.DateReceived, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.SenderName, act => act.MapFrom(src => src.Sender.Name))
                .ForMember(dest => dest.SenderGuid, act => act.MapFrom(src => src.Sender.UniqueId))
                .ForMember(dest => dest.ReceiverLoginName, act => act.MapFrom(src => src.ReceiverLoginName))
                .ForMember(dest => dest.DateRegistered, act => act.MapFrom(src => src.DateRegistered))
                .ForMember(dest => dest.DateSent, act => act.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.ReceiverName, act => act.MapFrom(src => src.Receiver.Name))
                .ForMember(dest => dest.DocReferenceNumber, act => act.MapFrom(src => src.DocReferenceNumber))
                .ForMember(dest => dest.ReceiverGuid, act => act.MapFrom(src => src.Receiver.UniqueId))
                .ForMember(dest => dest.SenderLoginName, act => act.MapFrom(src => src.SenderLoginName))
                .ForMember(dest => dest.RejectedReason, act => act.MapFrom(src => src.RejectedReason))
                .ForMember(dest => dest.AttentionTo, act => act.MapFrom(src => src.DocAttentionTo))
                .ForMember(dest => dest.RequestedCloseDate, act => act.MapFrom(src => src.DocReqDateClose))
                .ForMember(dest => dest.LastStatusChangeDate, act => act.MapFrom(src => src.DateUpdated))
                .ForMember(dest => dest.Subject, act => act.MapFrom(src => src.DocAbout))
                .ForMember(dest => dest.Comment, act => act.MapFrom(src => src.DocComment))
                .ForMember(dest => dest.DocAdditionalData, act => act.MapFrom(src => src.DocAddData))
                .ForMember(dest => dest.InternalRegIndex, act => act.MapFrom(src =>
                $"{src.DocNumberInternal} / {src.DateCreated.ToString("d MMM yyyy")}"))
                .ForMember(dest => dest.ExternalRegIndex, act => act.MapFrom(src =>
                !String.IsNullOrEmpty(src.DocNumberExternal)
                ? $"{src.DocNumberExternal} / {(src.DocDateExternal ?? src.DateCreated).ToString("d MMM yyyy")}"
                : null))
                .ForMember(dest => dest.DocGuid, act => act.MapFrom(src => src.DocGuid))
                .ForMember(dest => dest.DocKind, act => act.MapFrom(src => src.DocKind))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => (DocumentStatusType)src.Status))
                .ForMember(dest => dest.Service, act => act.MapFrom(src =>
                new MessageSerivceResponse { ServiceName = src.ServiceName, ServiceType = src.ServiceType, ServiceCode = src.ServiceCode }));

                cfg.CreateMap<RegisteredEntity, RegisteredEntityResponse>()
                .ForMember(dest => dest.UniqueIdentifier, act => act.MapFrom(src => src.UniqueId))
                .ForMember(dest => dest.AdministrationBodyName, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.EIK, act => act.MapFrom(src => src.EIK))
                .ForMember(dest => dest.Phone, act => act.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Emailddress, act => act.MapFrom(src => src.Email))
                .ForMember(dest => dest.ServiceUrl, act => act.MapFrom(src => src.ServiceUrl))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => (EntityServiceStatusEnum)src.Status));

                cfg.CreateMap<SEOSMessage, ChangeDocumentStatusResponse>()
                .ForMember(dest => dest.MessageId, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.OldStatus, act => act.MapFrom(src => (DocumentStatusType)src.Status))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => (DocumentStatusType)src.Status))
                .ForMember(dest => dest.ExpectedDateClose, act => act.MapFrom(src => src.DocExpectCloseDate))
                .ForMember(dest => dest.RejectReason, act => act.MapFrom(src => src.RejectedReason));

                cfg.CreateMap<SEOSMessage, SendMessageProperties>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.DocIdentity, act => act.MapFrom(src => 
                new DocumentIdentificationType
                {
                    DocumentGUID = src.DocGuid.ToString("B"),
                    Item = new DocumentNumberType() { DocNumber = src.DocNumberInternal, DocDate = src.DateCreated }
                }))
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => src.Receiver.Id))
                .ForMember(dest => dest.ReceiverServiceUrl, act => act.MapFrom(src => src.Receiver.ServiceUrl))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src =>
                src.Receiver))
                .ForMember(dest => dest.SenderCertificateSN, act => act.MapFrom(src => src.Sender.CertificateSN))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src =>
                src.Sender))
                .ForMember(dest => dest.MessageXml, act => act.Ignore());

                cfg.CreateMap<SEOSMessage, ReplyMessageProperties>()
                .IncludeBase<SEOSMessage, SendMessageProperties>()
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => src.Sender.Id))
                .ForMember(dest => dest.ReceiverServiceUrl, act => act.MapFrom(src => src.Sender.ServiceUrl))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src =>
                src.Sender != null 
                ? new EntityNodeType
                {
                    AdministrativeBodyName = src.Sender.Name,
                    GUID = src.Sender.UniqueId.ToString("B"),
                    Identifier = src.Sender.EIK
                } 
                : null))
                .ForMember(dest => dest.SenderCertificateSN, act => act.MapFrom(src => src.Receiver.CertificateSN))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => 
                src.Receiver != null 
                ? new EntityNodeType
                {
                    AdministrativeBodyName = src.Receiver.Name,
                    GUID = src.Receiver.UniqueId.ToString("B"),
                    Identifier = src.Receiver.EIK
                } 
                : null));

                cfg.CreateMap<CorespondentRequest, SEOSMessageCorespondent>()
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, act => act.MapFrom(src => src.Address))
                .ForMember(dest => dest.Bulstat, act => act.MapFrom(src => src.Bulstat))
                .ForMember(dest => dest.City, act => act.MapFrom(src => src.City))
                .ForMember(dest => dest.Email, act => act.MapFrom(src => src.Email))
                .ForMember(dest => dest.MobilePhone, act => act.MapFrom(src => src.MobilePhone));

                cfg.CreateMap<SEOSMessageAttachment, AttachmentRequest>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.Comment, act => act.MapFrom(src => src.Comment))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.Content, act => act.MapFrom(src => src.Content))
                .ForMember(dest => dest.MimeType, act => act.MapFrom(src => src.MimeType));

                cfg.CreateMap<Message, SendMessageProperties>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => 0))
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.Header.MessageGUID))
                .ForMember(dest => dest.DocIdentity, act => act.MapFrom(src =>src.GetDocIdentity()))
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => 0))
                .ForMember(dest => dest.ReceiverServiceUrl, act => act.MapFrom(src => String.Empty))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src => src.Header.Recipient))
                .ForMember(dest => dest.SenderCertificateSN, act => act.MapFrom(src => String.Empty))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => src.Header.Sender))
                .ForMember(dest => dest.MessageXml, act => act.Ignore());

                cfg.CreateMap<RedirectStatusRequestResult, SendMessageProperties>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => 0))
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.DocIdentity, act => act.MapFrom(src => src.DocIdentity))
                .ForMember(dest => dest.ReceiverId, act => act.MapFrom(src => 0))
                .ForMember(dest => dest.ReceiverServiceUrl, act => act.MapFrom(src => String.Empty))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src => src.Sender))
                .ForMember(dest => dest.SenderCertificateSN, act => act.MapFrom(src => String.Empty))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => src.Receiver))
                .ForMember(dest => dest.MessageXml, act => act.Ignore());

                cfg.CreateMap<Message, MessageCreationSettings>()
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.Header.MessageGUID))
                .ForMember(dest => dest.DocIdentity, act => act.MapFrom(src => src.GetDocIdentity()))
                .ForMember(dest => dest.RejectionReason, act => act.MapFrom(src => String.Empty))
                .ForMember(dest => dest.DocExpectCloseDateSpecified, act => act.MapFrom(src => false))
                .ForMember(dest => dest.DocExpectCloseDate, act => act.MapFrom(src => DateTime.MinValue))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src => src.Header.Sender))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src => src.Header.Recipient));

                cfg.CreateMap<SEOSMessage, MessageCreationSettings>()
                .ForMember(dest => dest.MessageGuid, act => act.MapFrom(src => src.MessageGuid))
                .ForMember(dest => dest.DocExpectCloseDateSpecified, act => act.MapFrom(src => src.DocExpectCloseDate.HasValue))
                .ForMember(dest => dest.RejectionReason, act => act.MapFrom(src => 
                src.Status == (int)DocumentStatusType.DS_REJECTED
                ? src.RejectedReason
                :null))
                .ForMember(dest => dest.DocExpectCloseDate, act => act.MapFrom(src => 
                src.DocExpectCloseDate.HasValue
                ? src.DocExpectCloseDate.Value
                :DateTime.MinValue))
                .ForMember(dest => dest.DocIdentity, act => act.MapFrom(src =>
                new DocumentIdentificationType
                {
                    DocumentGUID = src.DocGuid.ToString("B"),
                    Item = new DocumentNumberType() { DocNumber = src.DocNumberInternal, DocDate = src.DateCreated }
                }))
                .ForMember(dest => dest.Sender, act => act.MapFrom(src =>
                src.Sender))
                .ForMember(dest => dest.Receiver, act => act.MapFrom(src =>
                src.Receiver));

                cfg.CreateMap<CorrespondentType, SEOSMessageCorespondent>()
                .ForMember(dest => dest.Address, act => act.MapFrom(src => src.CorAddress))
                .ForMember(dest => dest.Bulstat, act => act.MapFrom(src => src.CorBULSTAT))
                .ForMember(dest => dest.City, act => act.MapFrom(src => src.CorCity))
                .ForMember(dest => dest.EGN, act => act.MapFrom(src => src.CorEGN))
                .ForMember(dest => dest.Email, act => act.MapFrom(src => src.CorEMail))
                .ForMember(dest => dest.IDCard, act => act.MapFrom(src => src.CorIDCard))
                .ForMember(dest => dest.MobilePhone, act => act.MapFrom(src => src.CorMobilePhone))
                .ForMember(dest => dest.MOL, act => act.MapFrom(src => src.CorMOL))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.CorName))
                .ForMember(dest => dest.Phone, act => act.MapFrom(src => src.CorPhone));

                cfg.CreateMap<AttachmentFileType, SEOSMessageAttachment>()
                .ForMember(dest => dest.Content, act => act.MapFrom(src => src.AttBody))
                .ForMember(dest => dest.Comment, act => act.MapFrom(src => src.AttComment))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.AttFileName))
                .ForMember(dest => dest.MimeType, act => act.MapFrom(src => src.AttMIMEType));

                cfg.CreateMap<RegisteredEntity, EntityNodeType>()
                .ForMember(dest => dest.AdministrativeBodyName, act => act.MapFrom(src => src.Name))
                .ForMember(dest => dest.GUID, act => act.MapFrom(src => src.UniqueId.ToString("B")))
                .ForMember(dest => dest.Identifier, act => act.MapFrom(src => src.EIK));
            });

            Mapping = new Mapper(config);
        }
    }
}
