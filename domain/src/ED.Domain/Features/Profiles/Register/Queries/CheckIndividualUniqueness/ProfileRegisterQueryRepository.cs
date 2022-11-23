using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileRegisterQueryRepository;

namespace ED.Domain
{
    partial class ProfileRegisterQueryRepository : IProfileRegisterQueryRepository
    {
        public async Task<CheckIndividualUniquenessVO> CheckIndividualUniquenessAsync(
            string identifier,
            string email,
            CancellationToken ct)
        {
            bool hasDuplicateIdentifier = await (
                from l in this.DbContext.Set<Login>()

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .AnyAsync(ct);


            bool hasDuplicateEmail = await (
                from l in this.DbContext.Set<Login>()

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                // TODO https://github.com/dotnet/efcore/issues/26634
#pragma warning disable CS8604 // Possible null reference argument.
                where (EF.Functions.Like(p.EmailAddress, email) || EF.Functions.Like(l.Email, email))
#pragma warning restore CS8604 // Possible null reference argument.
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .AnyAsync(ct);

            return new CheckIndividualUniquenessVO(
                !hasDuplicateIdentifier,
                !hasDuplicateEmail);
        }
    }
}
