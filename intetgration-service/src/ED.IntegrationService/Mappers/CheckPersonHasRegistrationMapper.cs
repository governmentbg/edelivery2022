using System.Linq;
using EDelivery.Common.DataContracts;
using EDelivery.Common.Enums;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcPersonRegistrationInfo ToDcPersonRegistrationInfo(
            ED.DomainServices.IntegrationService.CheckIndividualRegistrationResponse resp)
        {
            return new DcPersonRegistrationInfo
            {
                Name = resp.ProfileName,
                HasRegistration = resp.HasRegistration,
                PersonIdentificator = resp.ProfileIdentifier,
                AccessibleProfiles = resp.Profiles
                    .Select(e => new DcSubjectShortInfo
                    {
                        ProfileType = ToeProfileType(e.TargetGroupId),
                        Name = e.ProfileName,
                        EGN = ToeProfileType(e.TargetGroupId) == eProfileType.Person
                            ? e.ProfileIdentifier
                            : null,
                        EIK = ToeProfileType(e.TargetGroupId) != eProfileType.Person
                            ? e.ProfileIdentifier
                            : null,
                    })
                    .ToList()
            };
        }
    }
}
