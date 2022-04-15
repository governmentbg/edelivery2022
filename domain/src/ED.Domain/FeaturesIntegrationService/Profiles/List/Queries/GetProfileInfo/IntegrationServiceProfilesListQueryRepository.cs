using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<GetProfileInfoVO> GetProfileInfoAsync(
            string subjectId,
            CancellationToken ct)
        {
            Guid profileSubjectId = Guid.Parse(subjectId);

            int[] targetGroups = new int[]
            {
                TargetGroup.IndividualTargetGroupId,
                TargetGroup.LegalEntityTargetGroupId,
                TargetGroup.PublicAdministrationTargetGroupId,
                TargetGroup.SocialOrganizationTargetGroupId,
            };

            var profile = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where p.ElectronicSubjectId == profileSubjectId
                    && targetGroups.Contains(tgp.TargetGroupId)

                orderby p.IsActivated descending

                select new
                {
                    ProfileId = p.Id,
                    tgp.TargetGroupId
                })
                .FirstOrDefaultAsync(ct);

            if (profile == null)
            {
                return new GetProfileInfoVO(null, null, null);
            }
            else if (profile.TargetGroupId == TargetGroup.IndividualTargetGroupId)
            {
                GetProfileInfoVOIndividual vo = await (
                    from p in this.DbContext.Set<Profile>()

                    join i in this.DbContext.Set<Individual>()
                        on p.ElectronicSubjectId equals i.IndividualId

                    join a in this.DbContext.Set<Address>()
                        on p.AddressId equals a.AddressId
                        into lj1
                    from a in lj1.DefaultIfEmpty()

                    where p.Id == profile.ProfileId

                    select new GetProfileInfoVOIndividual(
                        p.ElectronicSubjectId.ToString(),
                        p.ElectronicSubjectName,
                        p.Identifier,
                        p.DateCreated,
                        p.IsActivated,
                        p.EmailAddress,
                        p.Phone,
                        i.FirstName,
                        i.MiddleName,
                        i.LastName,
                        a != null
                            ? new GetProfileInfoVOAddress(
                                a.AddressId,
                                a.Residence,
                                a.City,
                                a.State,
                                a.Country)
                            : null)
                        )
                    .SingleAsync(ct);

                return new GetProfileInfoVO(vo, null, null);
            }
            else if (profile.TargetGroupId == TargetGroup.LegalEntityTargetGroupId)
            {
                GetProfileInfoVOLegalEntity vo = await (
                   from p in this.DbContext.Set<Profile>()

                   join le in this.DbContext.Set<LegalEntity>()
                       on p.ElectronicSubjectId equals le.LegalEntityId

                   join a in this.DbContext.Set<Address>()
                     on p.AddressId equals a.AddressId
                     into lj1
                   from a in lj1.DefaultIfEmpty()

                   join l2 in this.DbContext.Set<Login>()
                        on p.CreatedBy equals l2.Id
                        into lj2
                   from l2 in lj2.DefaultIfEmpty()

                   join p2 in this.DbContext.Set<Profile>()
                        on l2.ElectronicSubjectId equals p2.ElectronicSubjectId
                        into lj3
                   from p2 in lj3.DefaultIfEmpty()

                   join i2 in this.DbContext.Set<Individual>()
                        on p2.ElectronicSubjectId equals i2.IndividualId
                        into lj4
                   from i2 in lj4.DefaultIfEmpty()

                   join a2 in this.DbContext.Set<Address>()
                     on p2.AddressId equals a2.AddressId
                     into lj5
                   from a2 in lj5.DefaultIfEmpty()

                   where p.Id == profile.ProfileId

                   select new GetProfileInfoVOLegalEntity(
                        p.ElectronicSubjectId.ToString(),
                        p.ElectronicSubjectName,
                        p.Identifier,
                        p.DateCreated,
                        p.IsActivated,
                        p.EmailAddress,
                        p.Phone,
                        le.Name,
                        p2 != null
                            ? new GetProfileInfoVOIndividual(
                                p2.ElectronicSubjectId.ToString(),
                                p2.ElectronicSubjectName,
                                p2.Identifier,
                                p2.DateCreated,
                                p2.IsActivated,
                                p2.EmailAddress,
                                p2.Phone,
                                i2 != null ? i2.FirstName : string.Empty, // i2 can be null for SystemProfile
                                i2 != null ? i2.MiddleName : string.Empty,
                                i2 != null ? i2.LastName : string.Empty,
                                a2 != null
                                    ? new GetProfileInfoVOAddress(
                                        a2.AddressId,
                                        a2.Residence,
                                        a2.City,
                                        a2.State,
                                        a2.Country)
                                    : null)
                            : null,
                        a != null
                            ? new GetProfileInfoVOAddress(
                                a.AddressId,
                                a.Residence,
                                a.City,
                                a.State,
                                a.Country)
                            : null))
                   .SingleAsync(ct);

                return new GetProfileInfoVO(null, vo, null);
            }
            else if (profile.TargetGroupId == TargetGroup.PublicAdministrationTargetGroupId
                || profile.TargetGroupId == TargetGroup.SocialOrganizationTargetGroupId)
            {
                var profileData = await (
                     from p in this.DbContext.Set<Profile>()

                     join le in this.DbContext.Set<LegalEntity>()
                         on p.ElectronicSubjectId equals le.LegalEntityId

                     join a in this.DbContext.Set<Address>()
                         on p.AddressId equals a.AddressId
                         into lj1
                     from a in lj1.DefaultIfEmpty()

                     join ple in this.DbContext.Set<LegalEntity>()
                         on le.ParentLegalEntityId equals ple.LegalEntityId
                         into lj2
                     from ple in lj2.DefaultIfEmpty()

                     join pp in this.DbContext.Set<Profile>()
                         on ple.LegalEntityId equals pp.ElectronicSubjectId
                         into lj3
                     from pp in lj3.DefaultIfEmpty()

                     join cle in this.DbContext.Set<LegalEntity>()
                         on le.LegalEntityId equals cle.ParentLegalEntityId
                         into lj4
                     from cle in lj4.DefaultIfEmpty()

                     join cp in this.DbContext.Set<Profile>()
                         on cle.LegalEntityId equals cp.ElectronicSubjectId
                         into lj5
                     from cp in lj5.DefaultIfEmpty()

                     where p.Id == profile.ProfileId

                     select new
                     {
                         p.ElectronicSubjectId,
                         p.ElectronicSubjectName,
                         p.Identifier,
                         p.DateCreated,
                         p.IsActivated,
                         p.EmailAddress,
                         p.Phone,
                         le.Name,
                         AddressId = a != null ? (int?)a.AddressId : null,
                         Residence = a != null ? (string?)a.Residence : null,
                         City = a != null ? (string?)a.City : null,
                         State = a != null ? (string?)a.State : null,
                         Country = a != null ? (string?)a.Country : null,
                         ParentElectronicSubjectId = pp != null ? (Guid?)pp.ElectronicSubjectId : null,
                         ParentElectronicSubjectName = pp != null ? (string?)pp.ElectronicSubjectName : null,
                         ParentIsActivated = pp != null ? (bool?)pp.IsActivated : null,
                         ParentEmailAddress = pp != null ? (string?)pp.EmailAddress : null,
                         ParentPhone = pp != null ? (string?)pp.Phone : null,

                         ChildElectronicSubjectId = cp != null ? (Guid?)cp.ElectronicSubjectId : null,
                         ChildElectronicSubjectName = cp != null ? (string?)cp.ElectronicSubjectName : null,
                         ChildIsActivated = cp != null ? (bool?)cp.IsActivated : null,
                         ChildEmailAddress = cp != null ? (string?)cp.EmailAddress : null,
                         ChildPhone = cp != null ? (string?)cp.Phone : null,
                     })
                     .ToArrayAsync(ct);

                GetProfileInfoVOAdministration vo = profileData
                    .GroupBy(e => new
                    {
                        e.ElectronicSubjectId,
                        e.ElectronicSubjectName,
                        e.Identifier,
                        e.DateCreated,
                        e.IsActivated,
                        e.EmailAddress,
                        e.Phone,
                        e.Name,
                        e.AddressId,
                        e.Residence,
                        e.City,
                        e.State,
                        e.Country,
                        e.ParentElectronicSubjectId,
                        e.ParentElectronicSubjectName,
                        e.ParentIsActivated,
                        e.ParentEmailAddress,
                        e.ParentPhone,
                    }, s => new
                    {
                        s.ChildElectronicSubjectId,
                        s.ChildElectronicSubjectName,
                        s.ChildIsActivated,
                        s.ChildEmailAddress,
                        s.ChildPhone,
                    })
                    .Select(e => new GetProfileInfoVOAdministration(
                        e.Key.ElectronicSubjectId.ToString(),
                        e.Key.ElectronicSubjectName,
                        e.Key.Identifier,
                        e.Key.DateCreated,
                        e.Key.IsActivated,
                        e.Key.EmailAddress,
                        e.Key.Phone,
                        e.Key.Name,
                        e.Key.ParentElectronicSubjectId != null
                            ? new GetProfileInfoVOAdministrationSibling(
                                 e.Key.ParentElectronicSubjectId.Value.ToString(),
                                 e.Key.ParentElectronicSubjectName,
                                 e.Key.ParentIsActivated!.Value,
                                 e.Key.ParentEmailAddress,
                                 e.Key.ParentPhone)
                            : null,
                        e
                            .Where(c => c.ChildElectronicSubjectId != null)
                            .Select(c => new GetProfileInfoVOAdministrationSibling(
                                c.ChildElectronicSubjectId!.Value.ToString(),
                                c.ChildElectronicSubjectName,
                                c.ChildIsActivated!.Value,
                                c.ChildEmailAddress,
                                c.ChildPhone))
                            .ToArray(),
                        e.Key.AddressId != null
                            ? new GetProfileInfoVOAddress(
                                e.Key.AddressId.Value,
                                e.Key.Residence,
                                e.Key.City,
                                e.Key.State,
                                e.Key.Country)
                            : null))
                    .First();

                return new GetProfileInfoVO(null, null, vo);
            }

            throw new NotImplementedException($"Unsupported target group {profile.TargetGroupId}");
        }
    }
}
