using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTemplatesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesCreateEditViewQueryRepository : IAdminTemplatesCreateEditViewQueryRepository
    {
        public async Task<ListProfilesVO[]> ListProfilesAsync(
            string term,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<Profile, bool>> predicate =
                PredicateBuilder.True<Profile>();

            if (!string.IsNullOrEmpty(term))
            {
                predicate = predicate
                    .And(p =>
                        EF.Functions.Like(p.ElectronicSubjectName, $"%{term}%")
                        || EF.Functions.Like(p.Identifier, $"%{term}%")
                        || EF.Functions.Like(p.EmailAddress, $"%{term}%"));
            }

            var result = await (
                    from p in this.DbContext.Set<Profile>().Where(predicate)

                    orderby p.Identifier, p.ElectronicSubjectName, p.EmailAddress
                    select new
                    {
                        p.Id,
                        p.Identifier,
                        p.ElectronicSubjectName,
                        p.EmailAddress
                    })
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync(ct);

            var profile = result
                .Select(x => new ListProfilesVO
                (
                    ProfileId: x.Id,
                    ProfileName: $"{x.Identifier} - {x.ElectronicSubjectName}{(!string.IsNullOrEmpty(x.EmailAddress) ? $" - {x.EmailAddress}" : string.Empty)}"
                ))
                .ToArray();

            return profile;
        }
    }
}
