using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileListQueryRepository;

namespace ED.Domain
{
    partial class ProfileListQueryRepository : IProfileListQueryRepository
    {
        public async Task<GetLoginProfilesVO[]> GetLoginProfilesAsync(
            int loginId,
            CancellationToken ct)
        {
            GetLoginProfilesVO[] vos = await (
                from p in this.DbContext.Set<Profile>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on p.Id equals lp.ProfileId

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId
                    into lj1
                from i in lj1.DefaultIfEmpty()

                join le in this.DbContext.Set<LegalEntity>()
                    on p.ElectronicSubjectId equals le.LegalEntityId
                    into lj2
                from le in lj2.DefaultIfEmpty()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where p.IsActivated
                    && !p.IsPassive
                    && lp.LoginId == loginId

                orderby lp.DateAccessGranted

                select new GetLoginProfilesVO(
                    p.Id,
                    (int)p.ProfileType,
                    p.ElectronicSubjectId.ToString(),
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone,
                    p.Identifier,
                    p.EnableMessagesWithCode,
                    tgp.TargetGroupId,
                    lp.IsDefault,
                    p.IsReadOnly,
                    lp.DateAccessGranted))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
