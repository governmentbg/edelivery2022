using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<TableResultVO<GetInstitutionsVO>> GetInstitutionsAsync(
            string term,
            int offset,
            int limit,
            CancellationToken ct)
        {
            int[] targetGroupIds = new int[]
            {
                TargetGroup.PublicAdministrationTargetGroupId,
                TargetGroup.SocialOrganizationTargetGroupId
            };

            Expression<Func<Profile, bool>> predicate =
                PredicateBuilder.True<Profile>();

            predicate = predicate.And(p => p.IsActivated);

            if (!string.IsNullOrEmpty(term))
            {
                predicate = predicate
                    .And(p =>
                        EF.Functions.Like(p.ElectronicSubjectName, $"%{term}%")
                        || EF.Functions.Like(p.Identifier, $"%{term}%"));
            }

            TableResultVO<GetInstitutionsVO> vos = await (
                from p in this.DbContext.Set<Profile>().Where(predicate)

                join le in this.DbContext.Set<LegalEntity>()
                    on p.ElectronicSubjectId equals le.LegalEntityId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where p.IsActivated
                    && !p.IsPassive
                    && !p.IsReadOnly
                    && targetGroupIds.Contains(tg.TargetGroupId)

                select new GetInstitutionsVO(
                    p.Id,
                    p.ElectronicSubjectName,
                    tg.TargetGroupId))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
