using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileInfoVO> GetProfileInfoAsync(
            int adminProfileId,
            int profileId,
            CancellationToken ct)
        {
            var profileInfo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId
                    into j1
                from i in j1.DefaultIfEmpty()

                join le in this.DbContext.Set<LegalEntity>()
                    on p.ElectronicSubjectId equals le.LegalEntityId
                    into j2
                from le in j2.DefaultIfEmpty()

                join l in this.DbContext.Set<Login>()
                    on p.CreatedBy equals l.Id
                    into j3
                from l in j3.DefaultIfEmpty()

                join addr in this.DbContext.Set<Address>()
                    on p.AddressId equals addr.AddressId
                    into j4
                from addr in j4.DefaultIfEmpty()

                join cntry in this.DbContext.Set<Country>()
                    on addr.Country equals cntry.CountryISO2
                    into j5
                from cntry in j5.DefaultIfEmpty()

                where p.Id == profileId

                select new
                {
                    p.ElectronicSubjectId,
                    p.IsActivated,
                    p.Identifier,
                    p.DateCreated,
                    p.Phone,
                    p.EmailAddress,
                    p.EnableMessagesWithCode,
                    p.IsReadOnly,
                    p.IsPassive,

                    tgp.TargetGroupId,
                    TargetGroupName = tg.Name,

                    i.FirstName,
                    i.MiddleName,
                    i.LastName,

                    LegalEntityName = le.Name,

                    CreatedByElectronicSubjectName = l.ElectronicSubjectName,

                    AddressCountry = cntry.Name,
                    AddressState = addr.State,
                    AddressCity = addr.City,
                    AddressResidence = addr.Residence,
                })
                .SingleAsync(ct);

            GetProfileInfoVOBlob[] blobs = await (
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

                select new GetProfileInfoVOBlob(
                    b.BlobId,
                    b.FileName,
                    pbak.Description,
                    b.CreateDate,
                    l != null
                        ? l.ElectronicSubjectName
                        : $"{ap.FirstName} {ap.LastName}"))
                .ToArrayAsync(ct);

            GetProfileInfoVOLogin[] logins = await (
                from lp in this.DbContext.Set<LoginProfile>()

                join l in this.DbContext.Set<Login>()
                    on lp.LoginId equals l.Id

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                join grantedBy in this.DbContext.Set<Login>()
                    on lp.AccessGrantedBy equals grantedBy.Id
                    into lj1
                from grantedBy in lj1.DefaultIfEmpty()

                join grantedByAdmin in this.DbContext.Set<AdminsProfile>()
                    on lp.AccessGrantedByAdminUserId equals grantedByAdmin.Id
                    into lj2
                from grantedByAdmin in lj2.DefaultIfEmpty()

                where lp.ProfileId == profileId

                select new GetProfileInfoVOLogin(
                    l.Id,
                    p.Id,
                    lp.IsDefault,
                    l.ElectronicSubjectName,
                    grantedBy != null
                        ? grantedBy.ElectronicSubjectName
                        : $"{grantedByAdmin.FirstName} {grantedByAdmin.LastName}",
                    lp.DateAccessGranted))
                .ToArrayAsync(ct);

            GetProfileInfoVORegistrationRequest[] registrationRequests = await (
                from rr in this.DbContext.Set<RegistrationRequest>()

                where rr.RegisteredProfileId == profileId

                orderby rr.CreateDate descending

                select new GetProfileInfoVORegistrationRequest(
                    rr.RegistrationRequestId,
                    rr.CreateDate,
                    rr.Status))
                .ToArrayAsync(ct);

            GetProfileInfoVOQuota? quota = await (
                from pq in this.DbContext.Set<ProfileQuota>()

                where pq.ProfileId == profileId

                select new GetProfileInfoVOQuota(pq.StorageQuotaInMb ?? ProfileQuota.DefaultStorageQuotaInMb))
                .FirstOrDefaultAsync(ct);

            if (quota == null)
            {
                quota = new GetProfileInfoVOQuota(ProfileQuota.DefaultStorageQuotaInMb);
            }

            GetProfileInfoVOEsbUser? esbUser = await (
                from peu in this.DbContext.Set<ProfileEsbUser>()

                where peu.ProfileId == profileId

                select new GetProfileInfoVOEsbUser(peu.OId, peu.ClientId))
                .FirstOrDefaultAsync(ct);

            if (esbUser == null)
            {
                esbUser = new GetProfileInfoVOEsbUser(null, null);
            }

            GetProfileInfoVOProfile[] profiles =
                Array.Empty<GetProfileInfoVOProfile>();

            GetProfileInfoVODefaultLogin? defaultLogin = null;

            if (profileInfo.TargetGroupId == TargetGroup.IndividualTargetGroupId)
            {
                profiles = await (
                    from p in this.DbContext.Set<Profile>()

                    join l in this.DbContext.Set<Login>()
                        on p.ElectronicSubjectId equals l.ElectronicSubjectId

                    join lp in this.DbContext.Set<LoginProfile>()
                        on l.Id equals lp.LoginId

                    join lpl in this.DbContext.Set<Login>()
                        on lp.AccessGrantedBy equals lpl.Id
                        into lj1
                    from lpl in lj1.DefaultIfEmpty()

                    join lpap in this.DbContext.Set<AdminsProfile>()
                        on lp.AccessGrantedByAdminUserId equals lpap.Id
                        into lj2
                    from lpap in lj2.DefaultIfEmpty()

                    join p2 in this.DbContext.Set<Profile>()
                        on lp.ProfileId equals p2.Id

                    join tgp2 in this.DbContext.Set<TargetGroupProfile>()
                        on p2.Id equals tgp2.ProfileId

                    join tg2 in this.DbContext.Set<TargetGroup>()
                        on tgp2.TargetGroupId equals tg2.TargetGroupId

                    where p.Id == profileId

                    select new GetProfileInfoVOProfile(
                        p2.Id,
                        p2.ElectronicSubjectName,
                        lpl != null
                            ? lpl.ElectronicSubjectName
                            : lpap != null
                                ? $"{lpap.FirstName} {lpap.LastName}"
                                : string.Empty,
                        tg2.Name,
                        p2.IsActivated))
                   .ToArrayAsync(ct);
            }
            else
            {
                defaultLogin = await (
                    from lp in this.DbContext.Set<LoginProfile>()

                    join l in this.DbContext.Set<Login>()
                        on lp.LoginId equals l.Id

                    join p in this.DbContext.Set<Profile>()
                        on l.ElectronicSubjectId equals p.ElectronicSubjectId

                    join grantedBy in this.DbContext.Set<Login>()
                        on lp.AccessGrantedBy equals grantedBy.Id
                        into lj1
                    from grantedBy in lj1.DefaultIfEmpty()

                    join grantedByAdmin in this.DbContext.Set<AdminsProfile>()
                        on lp.AccessGrantedByAdminUserId equals grantedByAdmin.Id
                        into lj2
                    from grantedByAdmin in lj2.DefaultIfEmpty()

                    where lp.ProfileId == profileId
                        && lp.IsDefault

                    select new GetProfileInfoVODefaultLogin(
                        l.Id,
                        l.IsActive,
                        l.CertificateThumbprint,
                        l.CanSendOnBehalfOf,
                        l.PushNotificationsUrl,
                        lp.SmsNotificationActive,
                        lp.SmsNotificationOnDeliveryActive,
                        lp.EmailNotificationActive,
                        lp.EmailNotificationOnDeliveryActive,
                        lp.ViberNotificationActive,
                        lp.ViberNotificationOnDeliveryActive,
                        lp.Email,
                        lp.Phone))
                    .FirstOrDefaultAsync(ct);
            }

            bool canBeActivated = false;
            if (!profileInfo.IsActivated)
            {
                canBeActivated =
                    !(await this.HasActiveOrPendingProfileAsync(
                        profileId,
                        profileInfo.Identifier,
                        profileInfo.TargetGroupId,
                        ct));
            }

            GetProfileInfoVOIndividualInfo? individualInfo = null;
            GetProfileInfoVOLegalEntityInfo? legalEntityInfo = null;
            if (profileInfo.TargetGroupId == TargetGroup.IndividualTargetGroupId)
            {
                individualInfo = new(
                    profileInfo.FirstName,
                    profileInfo.MiddleName,
                    profileInfo.LastName);
            }
            else
            {
                legalEntityInfo = new(profileInfo.LegalEntityName);
            }

            return new GetProfileInfoVO(
                canBeActivated,
                profileInfo.IsActivated,
                individualInfo,
                legalEntityInfo,
                profileInfo.Identifier,
                profileInfo.CreatedByElectronicSubjectName,
                profileInfo.DateCreated,
                profileInfo.Phone,
                profileInfo.EmailAddress,
                profileInfo.AddressCountry,
                profileInfo.AddressState,
                profileInfo.AddressCity,
                profileInfo.AddressResidence,
                profileInfo.TargetGroupId,
                profileInfo.TargetGroupName,
                profileInfo.EnableMessagesWithCode,
                blobs,
                logins,
                profiles,
                registrationRequests,
                profileInfo.IsReadOnly,
                profileInfo.IsPassive,
                defaultLogin,
                quota,
                esbUser);
        }
    }
}
