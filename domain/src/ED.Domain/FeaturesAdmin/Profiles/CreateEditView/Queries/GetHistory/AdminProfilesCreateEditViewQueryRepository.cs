using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<TableResultVO<GetHistoryVO>> GetHistoryAsync(
            int profileId,
            ProfileHistoryAction[] actions,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<ProfilesHistory, bool>> predicate =
                BuildPredicate(actions);

            TableResultVO<GetHistoryVO> vos = await (
                from ph in this.DbContext.Set<ProfilesHistory>().Where(predicate)

                join l in this.DbContext.Set<Login>()
                    on ph.ActionLogin equals l.Id
                    into lj1
                from l in lj1.DefaultIfEmpty()

                join ap in this.DbContext.Set<AdminsProfile>()
                    on ph.ActionByAdminUserId equals ap.Id
                    into lj2
                from ap in lj2.DefaultIfEmpty()

                where ph.ProfileId == profileId

                orderby ph.ActionDate descending

                select new GetHistoryVO(
                    ph.Id,
                    ph.ProfileId,
                    ph.ActionDate,
                    ph.Action,
                    l != null
                        ? l.ElectronicSubjectName
                        : null,
                    ph.ActionDetails,
                    ap != null
                        ? $"{ap.FirstName} {ap.LastName}"
                        : null,
                    ph.IPAddress))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<ProfilesHistory, bool>> BuildPredicate(
                ProfileHistoryAction[] actions)
            {
                Expression<Func<ProfilesHistory, bool>> predicate =
                    PredicateBuilder.True<ProfilesHistory>();

                if (actions.Any())
                {
                    predicate = predicate.And(e => actions.Contains(e.Action));
                }

                return predicate;
            }
        }
    }
}
