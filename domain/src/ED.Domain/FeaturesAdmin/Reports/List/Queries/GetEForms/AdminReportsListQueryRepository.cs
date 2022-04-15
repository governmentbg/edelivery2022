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

        public async Task<TableResultVO<GetEFormsVO>> GetEFormsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            string eFormServiceNumber,
            int offset,
            int limit,
            CancellationToken ct)
        {
            // carried over from old project
            // TODO: should we have a better way to log audit actions?
            this.logger.LogInformation($"{nameof(GetEFormsAsync)}({adminUserId}, \"{fromDate}\", \"{toDate}\", {offset}, {limit}) called");

            Expression<Func<Message, bool>> predicate =
                BuildMessagePredicate(
                    eFormServiceNumber,
                    fromDate,
                    toDate);

            TableResultVO<GetEFormsVO> vos = await (
                from m in this.DbContext.Set<Message>().Where(predicate)

                join l in this.DbContext.Set<Login>()
                    on m.SentViaLoginId equals l.Id

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                where p.Identifier == eFormsIdentifier

                group m by m.Subject into g

                orderby g.Key

                select new GetEFormsVO(g.Key, g.Count()))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<Message, bool>> BuildMessagePredicate(
               string eFormServiceNumber,
               DateTime fromDate,
               DateTime toDate)
            {
                // copy sp_ReportEFormsRequestedServices login and use DateCreated instead of DateSent
                Expression<Func<Message, bool>> predicate =
                    PredicateBuilder.True<Message>()
                    .And(e => e.DateCreated >= fromDate)
                    .And(e => e.DateCreated < toDate.AddDays(1));

                if (!string.IsNullOrEmpty(eFormServiceNumber))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.Subject, $"{eFormServiceNumber} %"));
                }

                return predicate;
            }
        }
    }
}
