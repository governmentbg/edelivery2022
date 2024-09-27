using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetProfileDataVO> GetProfileDataAsync(
            int adminProfileId,
            int profileId,
            CancellationToken ct)
        {
            var profileData = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                on p.Id equals tgp.ProfileId

                join i in this.DbContext.Set<Individual>()
                on p.ElectronicSubjectId equals i.IndividualId
                into j1
                from i in j1.DefaultIfEmpty()

                join le in this.DbContext.Set<LegalEntity>()
                on p.ElectronicSubjectId equals le.LegalEntityId
                into j2
                from le in j2.DefaultIfEmpty()

                join addr in this.DbContext.Set<Address>()
                on p.AddressId equals addr.AddressId
                into j4
                from addr in j4.DefaultIfEmpty()

                where p.Id == profileId

                select new
                {
                    p.Identifier,
                    p.Phone,
                    p.EmailAddress,
                    p.EnableMessagesWithCode,
                    p.IsActivated,
                    p.HideAsRecipient,

                    tgp.TargetGroupId,

                    i.FirstName,
                    i.MiddleName,
                    i.LastName,

                    LegalEntityName = le.Name,

                    AddressCountryCode = addr.Country,
                    AddressState = addr.State,
                    AddressCity = addr.City,
                    AddressResidence = addr.Residence,
                }
            ).SingleAsync(ct);

            GetProfileDataVOIndividualData? individualData = null;
            GetProfileDataVOLegalEntityData? legalEntityData = null;
            if (profileData.TargetGroupId == TargetGroup.IndividualTargetGroupId)
            {
                individualData =
                    new(
                        profileData.FirstName,
                        profileData.MiddleName,
                        profileData.LastName);
            }
            else
            {
                legalEntityData = new(profileData.LegalEntityName);
            }

            return new GetProfileDataVO(
                individualData,
                legalEntityData,
                profileData.Identifier,
                profileData.Phone,
                profileData.EmailAddress,
                profileData.AddressCountryCode,
                profileData.AddressState,
                profileData.AddressCity,
                profileData.AddressResidence,
                profileData.TargetGroupId,
                profileData.EnableMessagesWithCode,
                profileData.IsActivated,
                profileData.HideAsRecipient);
        }
    }
}
