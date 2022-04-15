using System;
using System.IO;
using System.Linq;
using EDelivery.Common.DataContracts;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcMessageDetails ToDcMessageDetails(
            ED.DomainServices.IntegrationService.GetSentMessageStatusResponse resp)
        {
            if (resp.Message == null)
            {
                return null;
            }

            return new DcMessageDetails
            {
                Id = resp.Message.MessageId,
                Title = resp.Message.ForwardedMessage != null
                    ? resp.Message.ForwardedMessage.MessageSubject
                    : resp.Message.MessageSubject,
                DateCreated = resp.Message.DateCreated.ToLocalDateTime(),
                DateSent = resp.Message.DateSent.ToLocalDateTime(),
                DateReceived = resp.Message.DateReceived?.ToLocalDateTime(),
                IsDraft = false,
                MessageText = resp.Message.ForwardedMessage != null
                    ? $"{resp.Message.ForwardedMessage.MessageBody}\r\n Препратено от {resp.Message.SenderProfile.ProfileName} с текст: {resp.Message.MessageBody}"
                    : resp.Message.MessageBody,
                FirstTimeOpen = resp.Message.FirstTimeOpen,
                TimeStampNRO = new DcTimeStamp(
                    resp.Message.TimestampNro.FileName,
                    resp.Message.TimestampNro.Content.ToByteArray()),
                TimeStampNRD = resp.Message.TimestampNrd != null
                    ? new DcTimeStamp(
                        resp.Message.TimestampNrd.FileName,
                        resp.Message.TimestampNrd.Content.ToByteArray())
                    : null,
                SenderProfile = resp.Message.ForwardedMessage != null
                    ? new DcProfile
                    {
                        Id = resp.Message.ForwardedMessage.SenderProfile.ProfileId,
                        IsDefault = false,
                        ElectronicSubjectId = Guid.Parse(resp.Message.ForwardedMessage.SenderProfile.ProfileSubjectId),
                        ElectronicSubjectName = resp.Message.ForwardedMessage.SenderProfile.ProfileName,
                        Email = resp.Message.ForwardedMessage.SenderProfile.Email,
                        Phone = resp.Message.ForwardedMessage.SenderProfile.Phone,
                        ProfileType = ToeProfileType(resp.Message.ForwardedMessage.SenderProfile.TargetGroupId),
                        DateCreated = resp.Message.ForwardedMessage.SenderProfile.DateCreated.ToLocalDateTime(),
                    }
                    : new DcProfile
                    {
                        Id = resp.Message.SenderProfile.ProfileId,
                        IsDefault = false,
                        ElectronicSubjectId = Guid.Parse(resp.Message.SenderProfile.ProfileSubjectId),
                        ElectronicSubjectName = resp.Message.SenderProfile.ProfileName,
                        Email = resp.Message.SenderProfile.Email,
                        Phone = resp.Message.SenderProfile.Phone,
                        ProfileType = ToeProfileType(resp.Message.SenderProfile.TargetGroupId),
                        DateCreated = resp.Message.SenderProfile.DateCreated.ToLocalDateTime(),
                    },
                SenderLogin = resp.Message.ForwardedMessage != null
                    ? new DcLogin
                    {
                        Id = resp.Message.ForwardedMessage.SenderLogin.LoginId,
                        ElectronicSubjectId = Guid.Parse(resp.Message.ForwardedMessage.SenderLogin.LoginSubjectId),
                        ElectronicSubjectName = resp.Message.ForwardedMessage.SenderLogin.LoginName,
                        Email = resp.Message.ForwardedMessage.SenderLogin.Email,
                        PhoneNumber = resp.Message.ForwardedMessage.SenderLogin.Phone,
                        IsActive = resp.Message.ForwardedMessage.SenderLogin.IsActive,
                        CertificateThumbprint = resp.Message.ForwardedMessage.SenderLogin.CertificateThumbprint,
                        PushNotificationsUrl = resp.Message.ForwardedMessage.SenderLogin.PushNotificationsUrl,
                    }
                    : new DcLogin
                    {
                        Id = resp.Message.SenderLogin.LoginId,
                        ElectronicSubjectId = Guid.Parse(resp.Message.SenderLogin.LoginSubjectId),
                        ElectronicSubjectName = resp.Message.SenderLogin.LoginName,
                        Email = resp.Message.SenderLogin.Email,
                        PhoneNumber = resp.Message.SenderLogin.Phone,
                        IsActive = resp.Message.SenderLogin.IsActive,
                        CertificateThumbprint = resp.Message.SenderLogin.CertificateThumbprint,
                        PushNotificationsUrl = resp.Message.SenderLogin.PushNotificationsUrl,
                    },
                ReceiverProfile = resp.Message.RecipientProfile != null
                    ? new DcProfile
                    {
                        Id = resp.Message.RecipientProfile.ProfileId,
                        IsDefault = false,
                        ElectronicSubjectId = Guid.Parse(resp.Message.RecipientProfile.ProfileSubjectId),
                        ElectronicSubjectName = resp.Message.RecipientProfile.ProfileName,
                        Email = resp.Message.RecipientProfile.Email,
                        Phone = resp.Message.RecipientProfile.Phone,
                        ProfileType = ToeProfileType(resp.Message.RecipientProfile.TargetGroupId),
                        DateCreated = resp.Message.RecipientProfile.DateCreated.ToLocalDateTime(),
                    }
                    : null,
                ReceiverLogin = resp.Message.RecipientLogin != null
                    ? new DcLogin
                    {
                        Id = resp.Message.RecipientLogin.LoginId,
                        ElectronicSubjectId = Guid.Parse(resp.Message.RecipientLogin.LoginSubjectId),
                        ElectronicSubjectName = resp.Message.RecipientLogin.LoginName,
                        Email = resp.Message.RecipientLogin.Email,
                        PhoneNumber = resp.Message.RecipientLogin.Phone,
                        IsActive = resp.Message.RecipientLogin.IsActive,
                        CertificateThumbprint = resp.Message.RecipientLogin.CertificateThumbprint,
                        PushNotificationsUrl = resp.Message.RecipientLogin.PushNotificationsUrl,
                    }
                    : null,
                TimeStampContent = new DcTimeStampMessageContent
                {
                    Content = resp.Message.TimestampMessage.Content.ToByteArray(),
                    FileName = resp.Message.TimestampMessage.FileName,
                    ContentType = "application/xml",
                },
                AttachedDocuments = resp.Message.ForwardedMessage != null
                    ? resp.Message.ForwardedMessage.Blobs
                        .Select(e => new DcDocument
                        {
                            Id = e.BlobId,
                            ContentEncodingCodePage = null,
                            ContentType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(e.FileName)),
                            DocumentRegistrationNumber = e.DocumentRegistrationNumber,
                            DocumentName = e.FileName,
                            TimeStamp = new DcTimeStamp(e.FileName, e.Timestamp.ToByteArray()),
                        })
                        .ToList()
                    : resp.Message.Blobs
                        .Select(e => new DcDocument
                        {
                            Id = e.BlobId,
                            ContentEncodingCodePage = null,
                            ContentType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(e.FileName)),
                            DocumentRegistrationNumber = e.DocumentRegistrationNumber,
                            DocumentName = e.FileName,
                            TimeStamp = new DcTimeStamp(e.FileName, e.Timestamp.ToByteArray()),
                        })
                        .ToList(),
            };
        }
    }
}
