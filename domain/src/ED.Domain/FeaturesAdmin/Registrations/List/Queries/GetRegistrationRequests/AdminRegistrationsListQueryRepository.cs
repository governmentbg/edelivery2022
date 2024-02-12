using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminRegistrationsListQueryRepository;

namespace ED.Domain
{
    partial class AdminRegistrationsListQueryRepository : IAdminRegistrationsListQueryRepository
    {
        public async Task<TableResultVO<GetRegistrationRequestsVO>> GetRegistrationRequestsAsync(
            int adminUserId,
            int? status,
            int offset,
            int limit,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, {offset}, {limit}) called",
                nameof(GetRegistrationRequestsAsync),
                adminUserId,
                offset,
                limit);

            Expression<Func<RegistrationRequest, bool>> predicate =
                PredicateBuilder.True<RegistrationRequest>();
            if (status.HasValue)
            {
                predicate = predicate
                    .And(e => e.Status == (RegistrationRequestStatus)status.Value);
            }

            TableResultVO<GetRegistrationRequestsVO> vos = await (
                from rr in this.DbContext.Set<RegistrationRequest>().Where(predicate)

                join p in this.DbContext.Set<Profile>()
                    on rr.RegisteredProfileId equals p.Id

                join l in this.DbContext.Set<Login>()
                    on rr.CreatedBy equals l.Id

                orderby rr.CreateDate descending

                select new GetRegistrationRequestsVO(
                    rr.RegistrationRequestId,
                    rr.Status,
                    p.Id,
                    p.ElectronicSubjectName,
                    l.ElectronicSubjectName,
                    rr.CreateDate))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
