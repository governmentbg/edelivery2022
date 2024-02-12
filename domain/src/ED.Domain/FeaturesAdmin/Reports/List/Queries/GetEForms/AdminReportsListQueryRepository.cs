using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        private const string eFormsIdentifier = "1770988090002";

        public async Task<GetEFormsVO[]> GetEFormsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            string? subject,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{fromDate}\", \"{toDate}\") called",
                nameof(GetEFormsAsync),
                adminUserId,
                fromDate,
                toDate);

            Expression<Func<Message, bool>> predicate =
                BuildMessagePredicate(
                    subject,
                    fromDate,
                    toDate);

            GetEFormsVO[] vos = await (
                from m in this.DbContext.Set<Message>().Where(predicate)

                join l in this.DbContext.Set<Login>()
                    on m.SentViaLoginId equals l.Id

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                where p.Identifier == eFormsIdentifier

                group m by new { m.Subject, m.RecipientsAsText }
                into g

                orderby g.Key.Subject

                select new GetEFormsVO(
                    g.Key.Subject,
                    g.Key.RecipientsAsText,
                    g.Count()))
                .ToArrayAsync(ct);

            return vos;

            Expression<Func<Message, bool>> BuildMessagePredicate(
               string? subject,
               DateTime fromDate,
               DateTime toDate)
            {
                // copy sp_ReportEFormsRequestedServices login and use DateCreated instead of DateSent
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>()
                    .And(e => e.DateCreated >= fromDate)
                    .And(e => e.DateCreated < toDate.AddDays(1));

                if (!string.IsNullOrEmpty(subject))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.Subject, $"{subject}%"));
                }

                return predicate;
            }
        }
    }
}
