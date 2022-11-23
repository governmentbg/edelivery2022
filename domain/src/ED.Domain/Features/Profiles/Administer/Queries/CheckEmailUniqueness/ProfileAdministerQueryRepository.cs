using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<bool> CheckEmailUniquenessAsync(
            string identifier,
            string email,
            CancellationToken ct)
        {
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
                    && p.Identifier != identifier
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .AnyAsync(ct);

            return !hasDuplicateEmail;
        }
    }
}
