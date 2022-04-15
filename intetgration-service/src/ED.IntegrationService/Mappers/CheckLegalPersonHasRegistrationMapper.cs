using System.Linq;
using EDelivery.Common.DataContracts;
using EDelivery.Common.Enums;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcLegalPersonRegistrationInfo ToDcLegalPersonRegistrationInfo(
            ED.DomainServices.IntegrationService.CheckLegalEntityRegistrationResponse resp)
        {
            return new DcLegalPersonRegistrationInfo
            {
                Name = resp.ProfileName,
                HasRegistration = resp.HasRegistration,
                EIK = resp.ProfileIdentifier,
                Email = resp.ProfileEmail,
                Phone = resp.ProfilePhone,
                ProfilesWithAccess = resp.Logins
                    .Select(e => new DcSubjectShortInfo
                    {
                        ProfileType = ToeProfileType(e.TargetGroupId),
                        Name = e.LoginName,
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
