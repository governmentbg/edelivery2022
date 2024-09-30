using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static ED.Domain.IIntegrationServiceMessagesOpenQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<GetSentMessageStatusAsRecipientVO> GetSentMessageStatusAsRecipientAsync(
            int profileId,
            int messageId,
            bool firstTimeOpen,
            CancellationToken ct)
        {
            var message = await (
                from m in this.DbContext.Set<Message>()

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { m.MessageId, ProfileId = profileId } equals new { mak.MessageId, mak.ProfileId }

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join stgp in this.DbContext.Set<TargetGroupProfile>()
                    on sp.Id equals stgp.ProfileId

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                   on mr.ProfileId equals rp.Id

                join rtgp in this.DbContext.Set<TargetGroupProfile>()
                    on rp.Id equals rtgp.ProfileId

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id

                where m.MessageId == messageId
                    && mr.ProfileId == profileId

                select new
                {
                    m.MessageId,
                    m.Subject,
                    m.Body,
                    m.DateCreated,
                    m.DateSent,
                    mr.DateReceived,
                    m.TemplateId,
                    m.TimeStampNRO,
                    m.MessageSummary,
                    TimeStampNRD = mr.Timestamp,
                    SenderProfileId = sp.Id,
                    SenderProfileSubjectId = sp.ElectronicSubjectId.ToString(),
                    SenderProfileName = sp.ElectronicSubjectName,
                    SenderProfileEmail = sp.EmailAddress,
                    SenderProfilePhone = sp.Phone,
                    SenderProfileDateCreated = sp.DateCreated,
                    SenderProfileTargetGroupId = stgp.TargetGroupId,
                    SenderLoginId = sl.Id,
                    SenderLoginSubjectId = sl.ElectronicSubjectId.ToString(),
                    SenderLoginName = sl.ElectronicSubjectName,
                    SenderLoginEmail = sl.Email,
                    SenderLoginPhone = sl.PhoneNumber,
                    SenderLoginIsActive = sl.IsActive,
                    SenderLoginCertificateThumbprint = sl.CertificateThumbprint,
                    SenderLoginPushNotificationUrl = sl.PushNotificationsUrl,
                    RecipientProfileId = rp.Id,
                    RecipientProfileSubjectId = rp.ElectronicSubjectId.ToString(),
                    RecipientProfileName = rp.ElectronicSubjectName,
                    RecipientProfileEmail = rp.EmailAddress,
                    RecipientProfilePhone = rp.Phone,
                    RecipientProfileDateCreated = rp.DateCreated,
                    RecipientProfileTargetGroupId = rtgp.TargetGroupId,
                    RecipientLoginId = rl.Id,
                    RecipientLoginSubjectId = rl.ElectronicSubjectId.ToString(),
                    RecipientLoginName = rl.ElectronicSubjectName,
                    RecipientLoginEmail = rl.Email,
                    RecipientLoginPhone = rl.PhoneNumber,
                    RecipientLoginIsActive = rl.IsActive,
                    RecipientLoginCertificateThumbprint = rl.CertificateThumbprint,
                    RecipientLoginPushNotificationUrl = rl.PushNotificationsUrl,
                    m.IV,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                })
                .FirstAsync(ct);

            var messageBlobs = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where mb.MessageId == messageId

                select new
                {
                    b.BlobId,
                    b.DocumentRegistrationNumber,
                    b.FileName,
                    b.Timestamp
                })
                .ToArrayAsync(ct);

            var forwardedMessage = await (
                from fm in this.DbContext.Set<ForwardedMessage>()

                join m in this.DbContext.Set<Message>()
                    on fm.ForwardedMessageId equals m.MessageId

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { m.MessageId, ProfileId = profileId } equals new { mak.MessageId, mak.ProfileId }

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join l in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l.Id

                where fm.MessageId == messageId

                select new
                {
                    m.MessageId,
                    m.Subject,
                    m.Body,
                    m.DateCreated,
                    m.DateSent,
                    m.TemplateId,
                    m.TimeStampNRO,
                    m.MessageSummary,
                    SenderProfileId = p.Id,
                    SenderProfileSubjectId = p.ElectronicSubjectId.ToString(),
                    SenderProfileName = p.ElectronicSubjectName,
                    SenderProfileEmail = p.EmailAddress,
                    SenderProfilePhone = p.Phone,
                    SenderProfileDateCreated = p.DateCreated,
                    SenderProfileTargetGroupId = tgp.TargetGroupId,
                    SenderLoginId = l.Id,
                    SenderLoginSubjectId = l.ElectronicSubjectId.ToString(),
                    SenderLoginName = l.ElectronicSubjectName,
                    SenderLoginEmail = l.Email,
                    SenderLoginPhone = l.PhoneNumber,
                    SenderLoginIsActive = l.IsActive,
                    SenderLoginCertificateThumbprint = l.CertificateThumbprint,
                    SenderLoginPushNotificationUrl = l.PushNotificationsUrl,
                    m.IV,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                })
                .FirstOrDefaultAsync(ct);

            GetSentMessageStatusAsRecipientVOBlob[] forwardedBlobs =
                Array.Empty<GetSentMessageStatusAsRecipientVOBlob>();

            if (forwardedMessage != null)
            {
                var forwardedMessageBlobs = await (
                    from fm in this.DbContext.Set<ForwardedMessage>()

                    join mb in this.DbContext.Set<MessageBlob>()
                        on fm.ForwardedMessageId equals mb.MessageId

                    join b in this.DbContext.Set<Blob>()
                        on mb.BlobId equals b.BlobId

                    where fm.MessageId == messageId

                    select new
                    {
                        b.BlobId,
                        b.DocumentRegistrationNumber,
                        b.FileName,
                        b.Timestamp
                    })
                    .ToArrayAsync(ct);

                int[] forwardedBlobIds = forwardedMessageBlobs.Select(e => e.BlobId).ToArray();

                forwardedBlobs =
                    new GetSentMessageStatusAsRecipientVOBlob[forwardedBlobIds.Length];

                for (int i = 0; i < forwardedBlobIds.Length; i++)
                {
                    var matchBlob = forwardedMessageBlobs.First(e => e.BlobId == forwardedBlobIds[i]);

                    forwardedBlobs[i] = new GetSentMessageStatusAsRecipientVOBlob(
                        matchBlob.BlobId,
                        matchBlob.DocumentRegistrationNumber,
                        matchBlob.FileName,
                        matchBlob.Timestamp);
                }
            }

            string messageBody = await GetMessageBodyAsRecipientAsync(
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                message.Body,
                message.TemplateId!.Value,
                ct);

            string? forwardedMessageBody = forwardedMessage != null
                ? await GetMessageBodyAsRecipientAsync(
                    forwardedMessage.ProfileKeyId,
                    forwardedMessage.EncryptedKey,
                    forwardedMessage.IV,
                    forwardedMessage.Body,
                    forwardedMessage.TemplateId!.Value,
                    ct)
                : null;

            return new GetSentMessageStatusAsRecipientVO(
                message.MessageId,
                message.Subject,
                message.DateCreated,
                message.DateSent!.Value,
                message.DateReceived,
                messageBody,
                new GetSentMessageStatusAsRecipientVOProfile(
                    message.SenderProfileId,
                    message.SenderProfileSubjectId,
                    message.SenderProfileName,
                    message.SenderProfileEmail,
                    message.SenderProfilePhone,
                    message.SenderProfileTargetGroupId,
                    message.SenderProfileDateCreated),
                new GetSentMessageStatusAsRecipientVOLogin(
                    message.SenderLoginId,
                    message.SenderLoginSubjectId,
                    message.SenderLoginName,
                    message.SenderLoginEmail,
                    message.SenderLoginPhone,
                    message.SenderLoginIsActive,
                    message.SenderLoginCertificateThumbprint,
                    message.SenderLoginPushNotificationUrl),
                new GetSentMessageStatusAsRecipientVOProfile(
                    message.RecipientProfileId,
                    message.RecipientProfileSubjectId,
                    message.RecipientProfileName,
                    message.RecipientProfileEmail,
                    message.RecipientProfilePhone,
                    message.RecipientProfileTargetGroupId,
                    message.RecipientProfileDateCreated),
                new GetSentMessageStatusAsRecipientVOLogin(
                    message.RecipientLoginId,
                    message.RecipientLoginSubjectId,
                    message.RecipientLoginName,
                    message.RecipientLoginEmail,
                    message.RecipientLoginPhone,
                    message.RecipientLoginIsActive,
                    message.RecipientLoginCertificateThumbprint,
                    message.RecipientLoginPushNotificationUrl),
                messageBlobs
                    .Select(e => new GetSentMessageStatusAsRecipientVOBlob(
                        e.BlobId,
                        e.DocumentRegistrationNumber,
                        e.FileName,
                        e.Timestamp))
                    .ToArray(),
                forwardedMessage != null
                    ? new GetSentMessageStatusAsRecipientVOForwardedMessage(
                        forwardedMessage.MessageId,
                        forwardedMessage.Subject,
                        forwardedMessage.DateCreated,
                        forwardedMessage.DateSent!.Value,
                        null,
                        forwardedMessageBody!,
                        new GetSentMessageStatusAsRecipientVOProfile(
                            forwardedMessage.SenderProfileId,
                            forwardedMessage.SenderProfileSubjectId,
                            forwardedMessage.SenderProfileName,
                            forwardedMessage.SenderProfileEmail,
                            forwardedMessage.SenderProfilePhone,
                            forwardedMessage.SenderProfileTargetGroupId,
                            forwardedMessage.SenderProfileDateCreated),
                        new GetSentMessageStatusAsRecipientVOLogin(
                            forwardedMessage.SenderLoginId,
                            forwardedMessage.SenderLoginSubjectId,
                            forwardedMessage.SenderLoginName,
                            forwardedMessage.SenderLoginEmail,
                            forwardedMessage.SenderLoginPhone,
                            forwardedMessage.SenderLoginIsActive,
                            forwardedMessage.SenderLoginCertificateThumbprint,
                            forwardedMessage.SenderLoginPushNotificationUrl),
                        null,
                        null,
                        forwardedBlobs)
                    : null,
                firstTimeOpen,
                new GetSentMessageStatusAsRecipientVOTimestampContent(
                    message.TimeStampNRO,
                    $"{message.DateCreated:yyyyMMdd}_{message.SenderProfileSubjectId}_{message.MessageId}_NRO.tsr"),
                new GetSentMessageStatusAsRecipientVOTimestampContent(
                    message.TimeStampNRD,
                    $"{message.DateCreated:yyyyMMdd}_{message.SenderProfileSubjectId}_{message.RecipientProfileSubjectId}_NRD.tsr"),
                new GetSentMessageStatusAsRecipientVOTimestampContent(
                    message.MessageSummary,
                    $"{message.DateCreated:yyyyMMdd}_{message.SenderProfileSubjectId}_{message.MessageId}.xml"));

            async Task<string> GetMessageBodyAsRecipientAsync(
                int profileKeyId,
                byte[] encryptedKey,
                byte[] iv,
                byte[] body,
                int templateId,
                CancellationToken ct)
            {
                ProfileKeyVO profileKey =
                  await this.profilesService.GetProfileKeyAsync(
                      profileKeyId,
                      ct);

                Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
                  await this.keystoreClient.DecryptWithRsaKeyAsync(
                      request: new Keystore.DecryptWithRsaKeyRequest
                      {
                          Key = new Keystore.RsaKey
                          {
                              Provider = profileKey.Provider,
                              KeyName = profileKey.KeyName,
                              OaepPadding = profileKey.OaepPadding,
                          },
                          EncryptedData = ByteString.CopyFrom(encryptedKey)
                      },
                      cancellationToken: ct);

                IEncryptor encryptor = this.encryptorFactory.CreateEncryptor(
                    decryptedKeyResp.Plaintext.ToByteArray(),
                    iv);

                string decryptedBody =
                    Encoding.UTF8.GetString(encryptor.Decrypt(body));

                Dictionary<Guid, object>? valuesDict =
                    JsonConvert.DeserializeObject<Dictionary<Guid, object>>(decryptedBody);

                Guid key;
                switch (templateId)
                {
                    case Template.SystemTemplateId:
                        key = Template.SystemTemplateBodyFieldId();
                        break;
                    case Template.SystemForwardTemplateId:
                        key = Template.SystemForwardTemplateBodyFieldId();
                        break;
                    default:
                        // ends methods
                        return string.Empty;
                }

                return valuesDict != null && valuesDict.ContainsKey(key)
                    ? (Convert.ToString(valuesDict[key]) ?? string.Empty)
                    : string.Empty;
            }
        }
    }
}
