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
        public async Task<GetSentMessageContentVO> GetSentMessageContentAsync(
            int profileId,
            int messageId,
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

                where m.MessageId == messageId
                    && m.SenderProfileId == profileId

                select new
                {
                    m.MessageId,
                    m.SubjectExtended,
                    m.Body,
                    m.DateCreated,
                    m.DateSent,
                    m.TemplateId,
                    m.TimeStampNRO,
                    m.MessageSummary,
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
                    m.IV,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                })
                .FirstAsync(ct);

            var recipient = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join rtgp in this.DbContext.Set<TargetGroupProfile>()
                    on rp.Id equals rtgp.ProfileId

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id
                    into lj1
                from rl in lj1.DefaultIfEmpty()

                where mr.MessageId == message.MessageId

                select new
                {
                    mr.DateReceived,
                    RecipientProfileId = rp.Id,
                    RecipientProfileSubjectId = rp.ElectronicSubjectId.ToString(),
                    RecipientProfileName = rp.ElectronicSubjectName,
                    RecipientProfileEmail = rp.EmailAddress,
                    RecipientProfilePhone = rp.Phone,
                    RecipientProfileDateCreated = rp.DateCreated,
                    RecipientProfileTargetGroupId = rtgp.TargetGroupId,
                    RecipientLoginId = rl != null ? (int?)rl.Id : null,
                    RecipientLoginSubjectId = rl != null ? (string?)rl.ElectronicSubjectId.ToString() : null,
                    RecipientLoginName = rl != null ? (string?)rl.ElectronicSubjectName : null,
                    RecipientLoginEmail = rl != null ? rl.Email : null,
                    RecipientLoginPhone = rl != null ? rl.PhoneNumber : null,
                    RecipientLoginIsActive = rl != null ? (bool?)rl.IsActive : null,
                    RecipientLoginCertificateThumbprint = rl != null ? rl.CertificateThumbprint : null,
                    RecipientLoginPushNotificationUrl = rl != null ? rl.PushNotificationsUrl : null,
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

            int[] blobIds = messageBlobs.Select(e => e.BlobId).ToArray();

            var messageBlobsSignatures = await (
                from bs in this.DbContext.Set<BlobSignature>()

                where this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == bs.BlobId)

                select new
                {
                    bs.BlobId,
                    bs.X509Certificate2DER,
                    bs.CoversDocument,
                    bs.CoversPriorRevision,
                    bs.IsTimestamp,
                    bs.SignDate,
                    bs.ValidAtTimeOfSigning,
                    bs.Issuer,
                    bs.Subject,
                    bs.SerialNumber,
                    bs.Version,
                    bs.ValidFrom,
                    bs.ValidTo,
                })
                .ToArrayAsync(ct);

            GetSentMessageContentVOBlob[] blobs =
                new GetSentMessageContentVOBlob[blobIds.Length];

            for (int i = 0; i < blobIds.Length; i++)
            {
                var matchBlob = messageBlobs.First(e => e.BlobId == blobIds[i]);

                blobs[i] = new GetSentMessageContentVOBlob(
                    matchBlob.BlobId,
                    matchBlob.DocumentRegistrationNumber,
                    matchBlob.FileName,
                    matchBlob.Timestamp,
                    messageBlobsSignatures
                        .Where(s => s.BlobId == blobIds[i])
                        .Select(s => new GetSentMessageContentVOSignature(
                            s.X509Certificate2DER ?? Array.Empty<byte>(),
                            s.CoversDocument,
                            s.CoversPriorRevision,
                            s.IsTimestamp,
                            s.SignDate,
                            s.ValidAtTimeOfSigning,
                            s.Issuer,
                            s.Subject,
                            s.SerialNumber,
                            s.Version,
                            s.ValidFrom,
                            s.ValidTo))
                        .ToArray());
            }

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

            GetSentMessageContentVOBlob[] forwardedBlobs =
                Array.Empty<GetSentMessageContentVOBlob>();

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

                var forwardedMessageBlobsSignatures = await (
                    from bs in this.DbContext.Set<BlobSignature>()

                    where this.DbContext.MakeIdsQuery(forwardedBlobIds).Any(id => id.Id == bs.BlobId)

                    select new
                    {
                        bs.BlobId,
                        bs.X509Certificate2DER,
                        bs.CoversDocument,
                        bs.CoversPriorRevision,
                        bs.IsTimestamp,
                        bs.SignDate,
                        bs.ValidAtTimeOfSigning,
                        bs.Issuer,
                        bs.Subject,
                        bs.SerialNumber,
                        bs.Version,
                        bs.ValidFrom,
                        bs.ValidTo,
                    })
                    .ToArrayAsync(ct);

                forwardedBlobs =
                    new GetSentMessageContentVOBlob[forwardedBlobIds.Length];

                for (int i = 0; i < forwardedBlobIds.Length; i++)
                {
                    var matchBlob = forwardedMessageBlobs.First(e => e.BlobId == forwardedBlobIds[i]);

                    forwardedBlobs[i] = new GetSentMessageContentVOBlob(
                        matchBlob.BlobId,
                        matchBlob.DocumentRegistrationNumber,
                        matchBlob.FileName,
                        matchBlob.Timestamp,
                        forwardedMessageBlobsSignatures
                            .Where(s => s.BlobId == forwardedBlobIds[i])
                            .Select(s => new GetSentMessageContentVOSignature(
                                s.X509Certificate2DER ?? Array.Empty<byte>(),
                                s.CoversDocument,
                                s.CoversPriorRevision,
                                s.IsTimestamp,
                                s.SignDate,
                                s.ValidAtTimeOfSigning,
                                s.Issuer,
                                s.Subject,
                                s.SerialNumber,
                                s.Version,
                                s.ValidFrom,
                                s.ValidTo))
                            .ToArray());
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

            return new GetSentMessageContentVO(
                message.MessageId,
                message.SubjectExtended,
                message.DateCreated,
                message.DateSent!.Value,
                recipient.DateReceived,
                messageBody,
                new GetSentMessageContentVOProfile(
                    message.SenderProfileId,
                    message.SenderProfileSubjectId,
                    message.SenderProfileName,
                    message.SenderProfileEmail,
                    message.SenderProfilePhone,
                    message.SenderProfileTargetGroupId,
                    message.SenderProfileDateCreated),
                new GetSentMessageContentVOLogin(
                    message.SenderLoginId,
                    message.SenderLoginSubjectId,
                    message.SenderLoginName,
                    message.SenderLoginEmail,
                    message.SenderLoginPhone,
                    message.SenderLoginIsActive,
                    message.SenderLoginCertificateThumbprint,
                    message.SenderLoginPushNotificationUrl),
                new GetSentMessageContentVOProfile(
                    recipient.RecipientProfileId,
                    recipient.RecipientProfileSubjectId,
                    recipient.RecipientProfileName,
                    recipient.RecipientProfileEmail,
                    recipient.RecipientProfilePhone,
                    recipient.RecipientProfileTargetGroupId,
                    recipient.RecipientProfileDateCreated),
                recipient.RecipientLoginId != null
                    ? new GetSentMessageContentVOLogin(
                        recipient.RecipientLoginId!.Value,
                        recipient.RecipientLoginSubjectId,
                        recipient.RecipientLoginName,
                        recipient.RecipientLoginEmail,
                        recipient.RecipientLoginPhone,
                        recipient.RecipientLoginIsActive!.Value,
                        recipient.RecipientLoginCertificateThumbprint,
                        recipient.RecipientLoginPushNotificationUrl)
                    : null,
                blobs,
                forwardedMessage != null
                    ? new GetSentMessageContentVOForwardedMessage(
                        forwardedMessage.MessageId,
                        forwardedMessage.Subject,
                        forwardedMessage.DateCreated,
                        forwardedMessage.DateSent!.Value,
                        null,
                        forwardedMessageBody!,
                        new GetSentMessageContentVOProfile(
                            forwardedMessage.SenderProfileId,
                            forwardedMessage.SenderProfileSubjectId,
                            forwardedMessage.SenderProfileName,
                            forwardedMessage.SenderProfileEmail,
                            forwardedMessage.SenderProfilePhone,
                            forwardedMessage.SenderProfileTargetGroupId,
                            forwardedMessage.SenderProfileDateCreated),
                        new GetSentMessageContentVOLogin(
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
                false,
                new GetSentMessageContentVOTimestampContent(
                    message.TimeStampNRO,
                    $"{message.DateCreated:yyyyMMdd}_{message.SenderProfileSubjectId}_{message.MessageId}_NRO.tsr"),
                null,
                new GetSentMessageContentVOTimestampContent(
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

                return valuesDict != null
                    ? (Convert.ToString(valuesDict[key]) ?? string.Empty)
                    : string.Empty;
            }
        }
    }
}
