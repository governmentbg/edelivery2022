using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminProfileListQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesListQueryRepository : IAdminProfileListQueryRepository
    {
        public async Task<TableResultVO<GetProfilesVO>> GetProfilesAsync(
            int adminUserId,
            string term,
            int offset,
            int limit,
            CancellationToken ct)
        {
            // carried over from old project
            // TODO: should we have a better way to log audit actions?
            this.logger.LogInformation($"{nameof(GetProfilesAsync)}({adminUserId}, \"{term}\", {offset}, {limit}) called");

            var predicate = PredicateBuilder.True<Profile>();
            if (!string.IsNullOrWhiteSpace(term))
            {
                string[] words = term.Split(null); // split by whitespace
                foreach (var word in words)
                {
                    predicate = predicate.AndAnyStringContains(
                        t => t.Identifier,
                        t => t.ElectronicSubjectName,
                        t => t.EmailAddress,
                        word);
                }
            }

            IQueryable<GetProfilesVO> query =
                from p in this.DbContext.Set<Profile>().Where(predicate)

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                orderby p.Id

                select new GetProfilesVO(
                    p.Id,
                    p.ProfileType,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.IsActivated,
                    tg.Name);

            TableResultVO<GetProfilesVO> table =
                await query.ToTableResultAsync(offset, limit, ct);

            return table;
        }
    }
}
