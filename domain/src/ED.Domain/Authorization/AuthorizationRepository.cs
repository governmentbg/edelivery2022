using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    class AuthorizationRepository : Repository, IAuthorizationRepository
    {
        public AuthorizationRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public async Task<bool> ExistsLoginProfileAsync(
            int loginId,
            int profileId,
            CancellationToken ct)
        {
            return await (
                from p in this.DbContext.Set<Profile>()
                join lp in this.DbContext.Set<LoginProfile>()
                on p.Id equals lp.ProfileId
                where lp.LoginId == loginId &&
                    p.Id == profileId &&
                    p.IsActivated
                select p
            ).AnyAsync(ct);
        }

        public async Task<bool> IsProfileReadOnlyAsync(
            int profileId,
            CancellationToken ct)
        {
            bool isReadOnly = await this.DbContext.Set<Profile>()
                .Where(e => e.Id == profileId && e.IsActivated)
                .Select(e => e.IsReadOnly)
                .SingleAsync(ct);

            return isReadOnly;
        }

        public async Task<bool> IsMessageSenderAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<Message>()
                .AnyAsync(
                    m => m.MessageId == messageId && m.SenderProfileId == profileId,
                    ct);
        }

        public async Task<bool> IsMessageRecipientAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<MessageRecipient>()
                .AnyAsync(
                    m => m.MessageId == messageId && m.ProfileId == profileId,
                    ct);
        }

        public async Task<int?> GetTemplateIdAsync(
            int messageId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<Message>()
                .Where(m => m.MessageId == messageId)
                .Select(m => m.TemplateId)
                .SingleAsync(ct);
        }

        public async Task<int?> GetForwardedMessageTemplateId(
            int messageId,
            CancellationToken ct)
        {
            return await (
                from fm in this.DbContext.Set<ForwardedMessage>()

                join m in this.DbContext.Set<Message>()
                    on fm.ForwardedMessageId equals m.MessageId

                where fm.MessageId == messageId

                select m.TemplateId)
                .SingleOrDefaultAsync(ct);
        }

        public async Task<int> GetResponseTemplateIdAsync(
            int messageId,
            CancellationToken ct)
        {
            int? responseTemplateId = await (
                from m in this.DbContext.Set<Message>()

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                where m.MessageId == messageId

                select t.ResponseTemplateId
            ).SingleAsync(ct);

            if (responseTemplateId == null)
            {
                responseTemplateId = await
                    this.DbContext.Set<Template>()
                    .Where(t => t.IsSystemTemplate)
                    .Select(t => t.TemplateId)
                    .SingleAsync(ct);
            }

            return responseTemplateId.Value;
        }

        public async Task<bool> HasMessageAccessKeyAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<MessageAccessKey>()
                .AnyAsync(
                    m => m.MessageId == messageId && m.ProfileId == profileId,
                    ct);
        }

        public async Task<bool> HasReadProfileMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<LoginProfilePermission>()
                .AnyAsync(
                    CreateMessageAccessPredicate(
                        loginId,
                        profileId,
                        templateId,
                        LoginProfilePermissionType.ReadProfileMessageAccess),
                    ct);
        }

        public async Task<bool> HasWriteProfileMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<LoginProfilePermission>()
                .AnyAsync(
                    CreateMessageAccessPredicate(
                        loginId,
                        profileId,
                        templateId,
                        LoginProfilePermissionType.WriteProfileMessageAccess),
                    ct);
        }

        public async Task<bool> HasAdministerProfileAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<LoginProfilePermission>()
                .AnyAsync(lpp =>
                    lpp.LoginId == loginId &&
                    lpp.ProfileId == profileId &&
                    new[]
                    {
                        LoginProfilePermissionType.OwnerAccess,
                        LoginProfilePermissionType.AdministerProfileAccess
                    }.Contains(lpp.Permission),
                    ct);
        }

        public async Task<bool> HasListProfileMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<LoginProfilePermission>()
                .AnyAsync(lpp =>
                    lpp.LoginId == loginId &&
                    lpp.ProfileId == profileId &&
                    new[]
                    {
                        LoginProfilePermissionType.OwnerAccess,
                        LoginProfilePermissionType.FullMessageAccess,
                        LoginProfilePermissionType.ListProfileMessageAccess
                    }.Contains(lpp.Permission),
                    ct);
        }

        public async Task<bool> HasAdministerProfileRecipientGroupAccessAsync(
            int profileId,
            int recipientGroupId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<RecipientGroup>()
                .AnyAsync(rg => rg.RecipientGroupId == recipientGroupId
                    && rg.ProfileId == profileId
                    && !rg.ArchiveDate.HasValue,
                    ct);
        }

        public async Task<bool> HasAccessTargetGroupSearchAsync(
            int profileId,
            int targetGroupId,
            CancellationToken ct)
        {
            return await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                join tgm in this.DbContext.Set<TargetGroupMatrix>()
                    on tg.TargetGroupId equals tgm.SenderTargetGroupId

                where p.Id == profileId
                    && p.IsActivated
                    && tgm.RecipientTargetGroupId == targetGroupId

                select tgm.SenderTargetGroupId)
                .AnyAsync(ct);
        }

        public async Task<bool> HasWriteProfileCodeMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            Expression<Func<LoginProfilePermission, bool>> predicate =
                CreateMessageAccessPredicate(
                    loginId,
                    profileId,
                    Template.SystemTemplateId,
                    LoginProfilePermissionType.WriteProfileMessageAccess);

            return await (
                from lpp in this.DbContext.Set<LoginProfilePermission>().Where(predicate)

                join p in this.DbContext.Set<Profile>()
                    on lpp.ProfileId equals p.Id

                where p.EnableMessagesWithCode.HasValue
                    && p.EnableMessagesWithCode.Value

                select lpp)
                .AnyAsync(ct);
        }

        public async Task<bool> IsForwardedMessage(
            int messageId,
            int forwardingMessageId,
            CancellationToken ct)
        {
            return await (
                from fm in this.DbContext.Set<ForwardedMessage>()

                where fm.MessageId == forwardingMessageId
                    && fm.ForwardedMessageId == messageId

                select fm.MessageId)
               .AnyAsync(ct);
        }

        private static Expression<Func<LoginProfilePermission, bool>> CreateMessageAccessPredicate(
            int loginId,
            int profileId,
            int templateId,
            LoginProfilePermissionType messageAccess)
        {
            return PredicateBuilder.True<LoginProfilePermission>()
                .And(lpp =>
                    lpp.LoginId == loginId
                    && lpp.ProfileId == profileId
                    && (
                        new[]
                        {
                            LoginProfilePermissionType.OwnerAccess,
                            LoginProfilePermissionType.FullMessageAccess
                        }.Contains(lpp.Permission)
                        || (
                            lpp.Permission == messageAccess
                            && lpp.TemplateId == templateId
                        )
                    ));
        }
    }
}
