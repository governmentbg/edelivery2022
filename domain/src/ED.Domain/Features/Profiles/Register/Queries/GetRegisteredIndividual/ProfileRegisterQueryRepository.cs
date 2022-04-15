using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileRegisterQueryRepository;

namespace ED.Domain
{
    partial class ProfileRegisterQueryRepository : IProfileRegisterQueryRepository
    {
        public async Task<GetRegisteredIndividualVO?> GetRegisteredIndividualAsync(
            string identifier,
            CancellationToken ct)
        {
            GetRegisteredIndividualVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                where p.ProfileType == ProfileType.Individual
                    && p.IsActivated
                    && EF.Functions.Like(p.Identifier, identifier)

                orderby p.IsActivated descending

                select new GetRegisteredIndividualVO(
                    p.Id,
                    p.ElectronicSubjectId.ToString(),
                    p.EmailAddress,
                    p.ElectronicSubjectName,
                    p.Phone,
                    p.Identifier))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
