using System;
using EDelivery.Common.DataContracts;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcSubjectRegistrationInfo ToDcSubjectRegistrationInfo(
            ED.DomainServices.IntegrationService.CheckProfileRegistrationResponse resp)
        {
            return new DcSubjectRegistrationInfo(resp.ProfileIdentifier)
            {
                HasRegistration = resp.HasRegistration,
                SubjectInfo = resp.HasRegistration ?
                new DcRegisteredSubjectInfo
                {
                    ElectronicSubjectId = Guid.Parse(resp.ProfileSubjectId),
                    ElectronicSubjectName = resp.ProfileName,
                    IsActivated = resp.ProfileIsActivated.Value,
                    Email = resp.ProfileEmail,
                    PhoneNumber = resp.ProfilePhone,
                    ProfileType = ToeProfileType(resp.TargetGroupId.Value),
                    InstitutionType = ToeInstitutionType(resp.TargetGroupId.Value),
                }
                : null,
            };
        }
    }
}
