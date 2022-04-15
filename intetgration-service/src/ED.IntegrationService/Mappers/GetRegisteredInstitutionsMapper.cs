using System;
using System.Collections.Generic;
using System.Linq;
using EDelivery.Common.DataContracts.ESubject;
using EDelivery.Common.Enums;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static List<DcInstitutionInfo> ToDcInstitutionInfo(
            ED.DomainServices.IntegrationService.GetRegisteredInstitutionsResponse resp)
        {
            return resp
                .Institutions
                .Select(e => new DcInstitutionInfo()
                {
                    ProfileType = eProfileType.Institution,
                    ElectronicSubjectId = Guid.Parse(e.SubjectId),
                    Address = e.Address != null
                        ? new DcAddress
                        {
                            Id = e.Address.AddressId,
                            Address = e.Address.Residence,
                            City = e.Address.City,
                            CountryIso2 = e.Address.Country,
                            State = e.Address.State,
                        }
                        : null,
                    DateCreated = e.DateCreated.ToDateTime(),
                    ElectronicSubjectName = e.Name,
                    Name = e.Name,
                    IsActivated = e.IsActivated,
                    Email = e.Email,
                    PhoneNumber = e.Phone,
                    UniqueSubjectIdentifier = e.Identifier,
                    HeadInstitution = e.Parent != null
                        ? new DcSubjectPublicInfo
                        {
                            ElectronicSubjectId = Guid.Parse(e.Parent.SubjectId),
                            ElectronicSubjectName = e.Parent.Name,
                            IsActivated = e.Parent.IsActivated,
                            Email = e.Parent.Email,
                            PhoneNumber = e.Parent.Phone,
                        }
                        : null,
                    SubInstitutions = e
                        .Children
                        .Select(si => new DcSubjectPublicInfo
                        {
                            ElectronicSubjectId = Guid.Parse(si.SubjectId),
                            ElectronicSubjectName = si.Name,
                            IsActivated = si.IsActivated,
                            Email = si.Email,
                            PhoneNumber = si.Phone
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}
