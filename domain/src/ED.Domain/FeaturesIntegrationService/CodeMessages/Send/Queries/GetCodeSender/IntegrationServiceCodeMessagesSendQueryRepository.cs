using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceCodeMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceCodeMessagesSendQueryRepository : IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public async Task<GetCodeSenderVO?> GetCodeSenderAsync(
            string certificateThumbprint,
            string? operatorIdentifier,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(operatorIdentifier))
            {
                GetCodeSenderVO? vo = await (
                    from l in this.DbContext.Set<Login>()

                    join lp in this.DbContext.Set<LoginProfile>()
                        on l.Id equals lp.LoginId

                    join p in this.DbContext.Set<Profile>()
                        on lp.ProfileId equals p.Id

                    where l.IsActive
                        && l.CertificateThumbprint == certificateThumbprint
                        && lp.IsDefault
                        && p.IsActivated
                        && (p.EnableMessagesWithCode ?? false)

                    select new GetCodeSenderVO(p.Id, l.Id))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
            else
            {
                GetCodeSenderVO? vo = await (
                    from l in this.DbContext.Set<Login>()

                    join lp in this.DbContext.Set<LoginProfile>()
                        on l.Id equals lp.LoginId

                    join p in this.DbContext.Set<Profile>()
                        on lp.ProfileId equals p.Id

                    join lp2 in this.DbContext.Set<LoginProfile>()
                        on p.Id equals lp2.ProfileId

                    join l2 in this.DbContext.Set<Login>()
                        on lp2.LoginId equals l2.Id

                    join p2 in this.DbContext.Set<Profile>()
                        on l2.ElectronicSubjectId equals p2.ElectronicSubjectId

                    where l.IsActive
                        && l.CertificateThumbprint == certificateThumbprint
                        && lp.IsDefault
                        && p.IsActivated
                        && (p.EnableMessagesWithCode ?? false)
                        && p2.Identifier == operatorIdentifier

                    select new GetCodeSenderVO(p.Id, l2.Id))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
        }
    }
}
