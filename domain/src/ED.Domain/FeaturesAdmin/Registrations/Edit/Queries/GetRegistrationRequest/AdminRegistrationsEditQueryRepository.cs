using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminRegistrationsEditQueryRepository;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<GetRegistrationRequestVO> GetRegistrationRequestAsync(
            int adminUserId,
            int registrationRequestId,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, {registrationRequestId}) called",
                nameof(GetRegistrationRequestAsync),
                adminUserId,
                registrationRequestId);

            var result = await (
                from rr in this.DbContext.Set<RegistrationRequest>()

                join p1 in this.DbContext.Set<Profile>()
                    on rr.RegisteredProfileId equals p1.Id

                join a in this.DbContext.Set<Address>()
                    on p1.AddressId equals a.AddressId

                join b in this.DbContext.Set<Blob>()
                    on rr.BlobId equals b.BlobId

                join l in this.DbContext.Set<Login>()
                    on rr.CreatedBy equals l.Id

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                    into j1
                from bs in j1.DefaultIfEmpty()

                join ap in this.DbContext.Set<AdminsProfile>()
                    on rr.ProcessedByAdminUserId equals ap.Id
                    into j2
                from ap in j2.DefaultIfEmpty()

                where rr.RegistrationRequestId == registrationRequestId

                select new
                {
                    RegistrationRequestCreateDate = rr.CreateDate,
                    RegistrationRequestStatus = rr.Status,
                    RegistrationRequestAuthor = l.ElectronicSubjectName,
                    RegistrationRequestEmail = rr.RegistrationEmail,
                    RegistrationRequestPhone = rr.RegistrationPhone,
                    RegistrationRequestProcessDate = rr.ProcessDate,
                    RegistrationRequestProcessUser = ap != null
                        ? $"{ap.FirstName} {ap.LastName}"
                        : null,
                    RegistrationRequestComment = rr.Comment,
                    ProfileId = p1.Id,
                    ProfileName = p1.ElectronicSubjectName,
                    ProfileIdentifier = p1.Identifier,
                    ProfileEmail = p1.EmailAddress,
                    ProfilePhone = p1.Phone,
                    ProfileResidence = a.Residence!,
                    RegistrationRequestBlobId = b.BlobId,
                    RegistrationRequestFileName = b.FileName,
                    IsValid = (bool?)bs.ValidAtTimeOfSigning,
                    SignedBy = (string?)bs.Subject,
                    CertifiedBy = (string?)bs.Issuer,
                    ValidFrom = (DateTime?)bs.ValidFrom,
                    ValidTo = (DateTime?)bs.ValidTo,
                })
                .ToArrayAsync(ct);

            GetRegistrationRequestVO vo =
                result.GroupBy(ri =>
                    new GetRegistrationRequestVO(
                        ri.RegistrationRequestCreateDate,
                        ri.RegistrationRequestStatus,
                        ri.RegistrationRequestAuthor,
                        ri.RegistrationRequestEmail,
                        ri.RegistrationRequestPhone,
                        ri.RegistrationRequestProcessDate,
                        ri.RegistrationRequestProcessUser,
                        ri.RegistrationRequestComment,
                        ri.ProfileId,
                        ri.ProfileName,
                        ri.ProfileIdentifier,
                        ri.ProfileEmail,
                        ri.ProfilePhone,
                        ri.ProfileResidence,
                        ri.RegistrationRequestBlobId,
                        ri.RegistrationRequestFileName,
                        Array.Empty<GetRegistrationRequestVOSignature>()))
                .Select(g =>
                    g.Key with
                    {
                        Signatures =
                            g.Where(gi => gi.IsValid != null)
                                .Select(gi =>
                                    new GetRegistrationRequestVOSignature(
                                        gi.IsValid!.Value,
                                        gi.SignedBy!,
                                        gi.CertifiedBy!,
                                        gi.ValidFrom!.Value,
                                        gi.ValidTo!.Value))
                                .ToArray()
                    })
                .Single();

            return vo;
        }
    }
}
