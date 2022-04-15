using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetBlobsVO>> GetBlobsAsync(
            int profileId,
            CancellationToken ct)
        {
            TableResultVO<GetBlobsVO> vos = await (
                from pbak in this.DbContext.Set<ProfileBlobAccessKey>()

                join b in this.DbContext.Set<Blob>()
                    on pbak.BlobId equals b.BlobId

                join l in this.DbContext.Set<Login>()
                    on pbak.CreatedByLoginId equals l.Id
                    into lj1
                from l in lj1.DefaultIfEmpty()

                join ap in this.DbContext.Set<AdminsProfile>()
                    on pbak.CreatedByAdminUserId equals ap.Id
                    into lj2
                from ap in lj2.DefaultIfEmpty()

                where pbak.ProfileId == profileId
                    && pbak.Type == ProfileBlobAccessKeyType.Registration

                orderby b.CreateDate descending

                select new GetBlobsVO(
                    b.BlobId,
                    b.FileName,
                    pbak.Description,
                    b.CreateDate,
                    l != null
                        ? l.ElectronicSubjectName
                        : $"{ap.FirstName} {ap.LastName}"))
                .ToTableResultAsync(null, null, ct);

            return vos;
        }
    }
}
