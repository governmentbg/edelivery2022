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
        public async Task<GetRegisteredInstitutionsVO[]> GetRegisteredInstitutionsAsync(
            CancellationToken ct)
        {
            int[] targetGroupIds = new int[]
             {
                TargetGroup.PublicAdministrationTargetGroupId,
                TargetGroup.SocialOrganizationTargetGroupId
             };

            GetRegisteredInstitutionsVO[] vos = (await (
                 from p in this.DbContext.Set<Profile>()

                 join tgp in this.DbContext.Set<TargetGroupProfile>()
                  on p.Id equals tgp.ProfileId

                 join tg in this.DbContext.Set<TargetGroup>()
                     on tgp.TargetGroupId equals tg.TargetGroupId

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

                 where targetGroupIds.Contains(tg.TargetGroupId)

                 select new
                 {
                     SubjectId = p.ElectronicSubjectId.ToString(),
                     p.Identifier,
                     p.DateCreated,
                     le.Name,
                     Email = p.EmailAddress,
                     p.Phone,
                     p.IsActivated,
                     p.AddressId,
                     Residence = a != null ? a.Residence : null,
                     City =a != null ? a.City : null,
                     State =a != null ? a.State : null,
                     Country = a != null ? a.Country : null,
                     ParentSubjectId = ple != null ? ple.LegalEntityId.ToString() : null,
                     ParentName = pp != null ? pp.ElectronicSubjectName : null,
                     ParentIsActivated = pp != null ? (bool?)pp.IsActivated : null,
                     ParentEmail = pp != null ? pp.EmailAddress : null,
                     ParentPhone = pp != null ? pp.Phone : null,
                     ChildSubjectId = cle != null ? cle.LegalEntityId.ToString() : null,
                     ChildName = cp != null ? cp.ElectronicSubjectName : null,
                     ChildIsActivated = cp != null ? (bool?)cp.IsActivated : null,
                     ChildEmail = cp != null ? cp.EmailAddress : null,
                     ChildPhone = cp != null ? cp.Phone : null,
                 })
                .ToArrayAsync(ct))
                .GroupBy(e => new
                {
                    e.SubjectId,
                    e.Identifier,
                    e.DateCreated,
                    e.Name,
                    e.Email,
                    e.Phone,
                    e.IsActivated,
                    e.AddressId,
                    e.Residence,
                    e.City,
                    e.State,
                    e.Country,
                    e.ParentSubjectId,
                    e.ParentName,
                    e.ParentIsActivated,
                    e.ParentEmail,
                    e.ParentPhone,
                },
                p => new {
                    p.ChildSubjectId,
                    p.ChildName,
                    p.ChildIsActivated,
                    p.ChildEmail,
                    p.ChildPhone,
                })
                .Select(e => new GetRegisteredInstitutionsVO(
                    e.Key.SubjectId,
                    e.Key.Identifier,
                    e.Key.DateCreated,
                    e.Key.Name,
                    e.Key.Email,
                    e.Key.Phone,
                    e.Key.IsActivated,
                    e.Key.AddressId != null 
                        ? new GetRegisteredInstitutionsVOAddress(
                            e.Key.AddressId!.Value,
                            e.Key.Residence,
                            e.Key.City,
                            e.Key.State,
                            e.Key.Country)
                        : null,
                    e.Key.ParentSubjectId != null
                        ? new GetRegisteredInstitutionsVOSibling(
                            e.Key.ParentSubjectId,
                            e.Key.ParentName,
                            e.Key.ParentIsActivated!.Value,
                            e.Key.ParentEmail,
                            e.Key.ParentPhone)
                        : null,
                    e
                        .Where(g => g.ChildSubjectId != null)
                        .Select(g => new GetRegisteredInstitutionsVOSibling(
                            g.ChildSubjectId,
                            g.ChildName,
                            g.ChildIsActivated!.Value,
                            g.ChildEmail,
                            g.ChildPhone))
                        .ToArray()
                    ))
                .ToArray();

            return vos;
        }
    }
}
