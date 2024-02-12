using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminProfileListQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesListQueryRepository : IAdminProfileListQueryRepository
    {
        public async Task<TableResultVO<GetProfilesVO>> GetProfilesAsync(
            int adminUserId,
            string? identifier,
            string? nameEmailPhone,
            int offset,
            int limit,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{identifier}\", \"{nameEmailPhone}\", {offset}, {limit}) called",
                nameof(GetProfilesAsync),
                adminUserId,
                identifier,
                nameEmailPhone,
                offset,
                limit);

            Expression<Func<Profile, bool>> predicate =
                BuildProfilePredicate(identifier, nameEmailPhone);

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
                    p.EmailAddress,
                    p.IsActivated,
                    tg.Name);

            TableResultVO<GetProfilesVO> table =
                await query.ToTableResultAsync(offset, limit, ct);

            return table;

            Expression<Func<Profile, bool>> BuildProfilePredicate(
                string? identifier,
                string? nameEmailPhone)
            {
                Expression<Func<Profile, bool>> predicate =
                    PredicateBuilder.True<Profile>();

                if (!string.IsNullOrEmpty(identifier))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.Identifier, $"%{identifier}%"));
                }

                if (!string.IsNullOrEmpty(nameEmailPhone))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.ElectronicSubjectName, $"%{nameEmailPhone}%")
                            || EF.Functions.Like(e.EmailAddress, $"%{nameEmailPhone}%")
                            || EF.Functions.Like(e.Phone, $"%{nameEmailPhone}%"));
                }

                return predicate;
            }
        }
    }
}
