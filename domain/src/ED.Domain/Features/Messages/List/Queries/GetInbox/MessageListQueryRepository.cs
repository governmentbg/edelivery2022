using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageListQueryRepository;

namespace ED.Domain
{
    partial class MessageListQueryRepository
    {
        public async Task<TableResultVO<GetInboxVO>> GetInboxAsync(
            int profileId,
            int offset,
            int limit,
            string? subject,
            string? profile,
            DateTime? fromDate,
            DateTime? toDate,
            string? rnu,
            CancellationToken ct)
        {
            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(
                    subject,
                    fromDate,
                    toDate,
                    rnu);

            Expression<Func<Profile, bool>> profilePredicate =
                BuildProfilePredicate(profile);

            IQueryable<int> countQuery =
                this.DbContext.Set<MessageRecipient>()
                    .Where(mr => mr.ProfileId == profileId)
                    .Select(mr => mr.MessageId);
            if (!profilePredicate.IsTrueLambdaExpr())
            {
                countQuery = countQuery.Where(
                    mId =>
                        (from m in this.DbContext.Set<Message>().Where(messagePredicate)
                         join p1 in this.DbContext.Set<Profile>().Where(profilePredicate)
                             on m.SenderProfileId equals p1.Id
                         where m.MessageId == mId
                         select m)
                         .Any());
            }
            else if (!messagePredicate.IsTrueLambdaExpr())
            {
                countQuery = countQuery.Where(
                    mId =>
                        (from m in this.DbContext.Set<Message>().Where(messagePredicate)
                        where m.MessageId == mId
                        select m)
                        .Any());
            }

            int count = await countQuery.CountAsync(ct);

            if (count == 0)
            {
                return TableResultVO.Empty<GetInboxVO>();
            }

            // Use the DateTime value of the SqlDateTime.MaxValue.
            // EF serializes the SqlDateTime to string which ends up as
            // '31-Dec-99 23:59:59', which is not the intended 9999 year.
            DateTime sqlMaxDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

            GetInboxVO[] vos = await (
                from m in this.DbContext.Set<Message>().Where(messagePredicate)

                join p1 in this.DbContext.Set<Profile>().Where(profilePredicate)
                    on m.SenderProfileId equals p1.Id

                join l1 in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l1.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join p2 in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p2.Id

                join l2 in this.DbContext.Set<Login>()
                    on mr.LoginId equals l2.Id
                    into lj1
                from l2 in lj1.DefaultIfEmpty()

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                where mr.ProfileId == profileId

                orderby mr.DateReceived ?? sqlMaxDate descending, m.DateSent descending

                select new GetInboxVO(
                    m.MessageId,
                    m.DateSent!.Value,
                    mr.DateReceived,
                    p1.ElectronicSubjectName,
                    l1.ElectronicSubjectName,
                    p2.ElectronicSubjectName,
                    l2 != null ? l2.ElectronicSubjectName : string.Empty,
                    m.SubjectExtended!,
                    m.ForwardStatusId,
                    t.Name,
                    m.Rnu))
                .WithOffsetAndLimit(offset, limit)
                .ToArrayAsync(ct);

            return new TableResultVO<GetInboxVO>(vos, count);

            Expression<Func<Message, bool>> BuildMessagePredicate(
                string? subject,
                DateTime? fromDate,
                DateTime? toDate,
                string? rnu)
            {
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>();

                if (!string.IsNullOrEmpty(subject))
                {
                    predicate = predicate
                        .And(e => e.Subject.Contains(subject));
                }

                if (fromDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent.Value > fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.DateSent.HasValue && e.DateSent < toDate.Value.AddDays(1));
                }

                if (!string.IsNullOrEmpty(rnu))
                {
                    predicate = predicate
                        .And(e => e.Rnu == rnu);
                }

                return predicate;
            }

            Expression<Func<Profile, bool>> BuildProfilePredicate(
                string? profile)
            {
                Expression<Func<Profile, bool>> predicate =
                    PredicateBuilder.True<Profile>();

                if (!string.IsNullOrEmpty(profile))
                {
                    predicate = predicate
                        .And(e => e.ElectronicSubjectName.Contains(profile)
                            || e.Identifier.Contains(profile));
                }

                return predicate;
            }
        }
    }
}
