using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITicketsOpenQueryRepository;

namespace ED.Domain
{
    partial class TicketsOpenQueryRepository : ITicketsOpenQueryRepository
    {
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            string query = $@"
SELECT TOP(1)
    [m].[MessageId],
    [m].[DateSent],
    [m0].[DateReceived],
    [p].[Id] AS [SenderProfileId],
    [p].[ElectronicSubjectName] AS [SenderProfileName],
    [p].[ProfileType] AS [SenderProfileType],
    [p].[IsReadOnly] AS [SenderProfileIsReadOnly],
    [l].[ElectronicSubjectName] AS [SenderLoginName],
    [p0].[Id] AS [RecipientProfileId],
    [p0].[ElectronicSubjectName] AS [RecipientProfileName],
    [p0].[ProfileType] AS [RecipientProfileType],
    [p0].[IsReadOnly] AS [RecipientProfileIsReadOnly],
    [l0].[ElectronicSubjectName] AS [RecipientLoginName],
    [m].[Subject],
    [m].[Body],
    [m1].[ProfileKeyId],
    [m1].[EncryptedKey],
    [m].[IV],
    [t].[Type],
    [t].[DocumentIdentifier],
    [e].[Status],
    [e].[ServeDate],
    [e].[AnnulDate],
    [e].[AnnulmentReason]
FROM [dbo].[Messages] AS [m]
INNER JOIN [dbo].[Tickets] AS [t] ON [m].[MessageId] = [t].[MessageId]
CROSS APPLY (
    SELECT TOP 1
        [ts].[Status],
        [ts].[ServeDate],
        [ts].[AnnulDate],
        [ts].[AnnulmentReason]
    FROM TicketStatuses [ts]
    WHERE [ts].[MessageId] = [t].[MessageId]
    ORDER BY [ts].[TicketStatusId] DESC
) AS [e]
INNER JOIN [dbo].[Profiles] AS [p] ON [m].[SenderProfileId] = [p].[Id]
INNER JOIN [dbo].[Logins] AS [l] ON [m].[SenderLoginId] = [l].[Id]
INNER JOIN [dbo].[MessageRecipients] AS [m0] ON [m].[MessageId] = [m0].[MessageId]
INNER JOIN [dbo].[Profiles] AS [p0] ON [m0].[ProfileId] = [p0].[Id]
INNER JOIN [dbo].[Logins] AS [l0] ON [m0].[LoginId] = [l0].[Id]
INNER JOIN [dbo].[MessageAccessKeys] AS [m1] ON [m].[MessageId] = [m1].[MessageId]
LEFT JOIN [dbo].[ForwardedMessages] AS [f] ON [m].[MessageId] = [f].[MessageId]
WHERE
    [m].[MessageId] = @messageId
    AND [m0].[ProfileId] = @profileId
    AND [m1].[ProfileId] = @profileId
";

            SqlParameter[] parameters = new[]
            {
                new SqlParameter("messageId", SqlDbType.Int) { Value = messageId },
                new SqlParameter("profileId", SqlDbType.Int) { Value = profileId },
            };

            MessageAsRecipientQO message =
                await this.DbContext.Set<MessageAsRecipientQO>()
                    .FromSqlRaw(query, parameters)
                    .AsNoTracking()
                    .FirstAsync(ct);

            var blobsAndSignatures = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                    into lj3
                from msr in lj3.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                    into lj4
                from bs in lj4.DefaultIfEmpty()

                where mb.MessageId == messageId

                select new
                {
                    b.BlobId,
                    b.FileName,
                    b.Size,
                    b.DocumentRegistrationNumber,
                    msr.Status,
                    msr.IsMalicious,
                    b.Hash,
                    b.HashAlgorithm,
                    SignatureCoversDocument = (bool?)bs.CoversDocument,
                    SignatureSignDate = (DateTime?)bs.SignDate,
                    SignatureIsTimestamp = (bool?)bs.IsTimestamp,
                    SignatureValidAtTimeOfSigning = (bool?)bs.ValidAtTimeOfSigning,
                    SignatureIssuer = (string?)bs.Issuer,
                    SignatureSubject = (string?)bs.Subject,
                    SignatureValidFrom = (DateTime?)bs.ValidFrom,
                    SignatureValidTo = (DateTime?)bs.ValidTo,
                })
                .ToArrayAsync(ct);

            GetAsRecipientVOBlob blob = blobsAndSignatures
                .GroupBy(e => e.BlobId)
                .Select(e => new GetAsRecipientVOBlob(
                    e.Key,
                    e.First().FileName,
                    e.First().Hash,
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().Status ?? MalwareScanResultStatus.Error,
                    e.First().IsMalicious,
                    e
                        .Where(s => s.SignatureCoversDocument.HasValue)
                        .Select(s => new GetAsRecipientVOBlobSignature(
                            s.SignatureCoversDocument!.Value,
                            s.SignatureSignDate!.Value,
                            s.SignatureIsTimestamp!.Value,
                            s.SignatureValidAtTimeOfSigning!.Value,
                            s.SignatureIssuer,
                            s.SignatureSubject,
                            s.SignatureValidFrom!.Value,
                            s.SignatureValidTo!.Value))
                        .ToArray()))
                .First();

            GetAsRecipientVO vo = new(
                message.MessageId,
                message.DateSent,
                message.DateReceived,
                new GetAsRecipientVOTicket(
                    message.Type,
                    new GetAsRecipientVOTicketStatus(
                        message.Status,
                        message.ServeDate,
                        message.AnnulDate,
                        message.AnnulmentReason)),
                new GetAsRecipientVOProfile(
                    message.SenderProfileId,
                    message.SenderProfileName,
                    message.SenderProfileType,
                    message.SenderProfileIsReadOnly,
                    message.SenderLoginName),
                new GetAsRecipientVOProfile(
                    message.RecipientProfileId,
                    message.RecipientProfileName,
                    message.RecipientProfileType,
                    message.RecipientProfileIsReadOnly,
                    message.RecipientLoginName),
                message.Subject,
                message.Body,
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                blob,
                !string.IsNullOrEmpty(message.DocumentIdentifier)
                    ? $"p={message.RecipientProfileId}&t={message.MessageId}".ToUrlSafeBase64()
                    : string.Empty);

            return vo;
        }
    }
}
