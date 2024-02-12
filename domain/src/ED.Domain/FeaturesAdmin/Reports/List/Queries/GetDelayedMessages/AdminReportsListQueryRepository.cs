using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        public async Task<TableResultVO<GetDelayedMessagesVO>> GetDelayedMessagesAsync(
            int adminUserId,
            int delay,
            int targetGroupId,
            int? profileId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{delay}\", \"{targetGroupId}\", \"{profileId}\", {offset}, {limit}) called",
                nameof(GetDelayedMessagesVO),
                adminUserId,
                delay,
                targetGroupId,
                profileId,
                offset,
                limit);

            Expression<Func<MessageRecipient, bool>> messageRecipientPredicate =
                BuildMessageRecipientPredicate(profileId);

            DateTime now = DateTime.Now;
            DateTime filter = now.Date.AddDays(-delay);

            TableResultVO<GetDelayedMessagesVO> vos = await (
                from mr in this.DbContext.Set<MessageRecipient>().Where(messageRecipientPredicate)

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join m in this.DbContext.Set<Message>()
                    on mr.MessageId equals m.MessageId

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join rtgp in this.DbContext.Set<TargetGroupProfile>()
                    on rp.Id equals rtgp.ProfileId

                join rtg in this.DbContext.Set<TargetGroup>()
                    on rtgp.TargetGroupId equals rtg.TargetGroupId

                where rp.IsActivated
                      && rtgp.TargetGroupId == targetGroupId
                      && m.DateSent < filter

                orderby rp.ElectronicSubjectName

                select new GetDelayedMessagesVO(
                    rp.Id,
                    rp.ElectronicSubjectName,
                    rp.IsActivated,
                    rtg.Name,
                    rp.EmailAddress,
                    sp.Id,
                    sp.ElectronicSubjectName,
                    sp.EmailAddress,
                    m.SubjectExtended,
                    m.DateSent,
                    (now.Date - m.DateSent!.Value.Date).Days))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<MessageRecipient, bool>> BuildMessageRecipientPredicate(
                int? profileId)
            {
                Expression<Func<MessageRecipient, bool>> predicate =
                    PredicateBuilder.True<MessageRecipient>()
                        .And(mr => mr.DateReceived == null);

                if (profileId.HasValue)
                {
                    predicate = predicate
                        .And(e => e.ProfileId == profileId.Value);
                }

                return predicate;
            }
        }
    }
}
