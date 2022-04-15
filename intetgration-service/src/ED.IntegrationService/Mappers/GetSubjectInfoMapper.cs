using System;
using System.Linq;
using EDelivery.Common.DataContracts.ESubject;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcSubjectInfo ToDcSubjectInfo(
            ED.DomainServices.IntegrationService.GetProfileInfoResponse resp)
        {
            if (resp.Individual != null)
            {
                return new DcPersonInfo()
                {
                    ElectronicSubjectId = Guid.Parse(resp.Individual.ProfileSubjectId),
                    ElectronicSubjectName = resp.Individual.ProfileName,
                    UniqueSubjectIdentifier = resp.Individual.ProfileIdentifier,
                    DateCreated = resp.Individual.DateCreated.ToLocalDateTime(),
                    IsActivated = resp.Individual.IsActivated,
                    Email = resp.Individual.Email,
                    PhoneNumber = resp.Individual.Phone,
                    BirthDate = DateTime.MinValue,
                    DateOfDeath = null,
                    FirstName = resp.Individual.FirstName,
                    MiddleName = resp.Individual.MiddleName,
                    LastName = resp.Individual.LastName,
                    Address = resp.Individual.Address != null
                        ? new DcAddress
                        {
                            Id = resp.Individual.Address.AddressId,
                            Address = resp.Individual.Address.Residence,
                            City = resp.Individual.Address.City,
                            State = resp.Individual.Address.State,
                            CountryIso2 = resp.Individual.Address.Country
                        }
                        : null,
                };
            }
            else if (resp.LegalEntity != null)
            {
                return new DcLegalPersonInfo()
                {
                    ElectronicSubjectId = Guid.Parse(resp.LegalEntity.ProfileSubjectId),
                    ElectronicSubjectName = resp.LegalEntity.ProfileName,
                    UniqueSubjectIdentifier = resp.LegalEntity.ProfileIdentifier,
                    DateCreated = resp.LegalEntity.DateCreated.ToLocalDateTime(),
                    IsActivated = resp.LegalEntity.IsActivated,
                    Email = resp.LegalEntity.Email,
                    PhoneNumber = resp.LegalEntity.Phone,
                    CompanyName = resp.LegalEntity.Name,
                    InForceDate = DateTime.MinValue,
                    DateOutOfForce = null,
                    RegisteredBy = resp.LegalEntity.RegisteredBy != null
                        ? new DcPersonInfo()
                        {
                            ElectronicSubjectId = Guid.Parse(resp.LegalEntity.RegisteredBy.ProfileSubjectId),
                            ElectronicSubjectName = resp.LegalEntity.RegisteredBy.ProfileName,
                            UniqueSubjectIdentifier = resp.LegalEntity.RegisteredBy.ProfileIdentifier,
                            DateCreated = resp.LegalEntity.RegisteredBy.DateCreated.ToLocalDateTime(),
                            IsActivated = resp.LegalEntity.RegisteredBy.IsActivated,
                            Email = resp.LegalEntity.RegisteredBy.Email,
                            PhoneNumber = resp.LegalEntity.RegisteredBy.Phone,
                            BirthDate = DateTime.MinValue,
                            DateOfDeath = null,
                            FirstName = resp.LegalEntity.RegisteredBy.FirstName,
                            MiddleName = resp.LegalEntity.RegisteredBy.MiddleName,
                            LastName = resp.LegalEntity.RegisteredBy.LastName,
                            Address = resp.LegalEntity.RegisteredBy.Address != null
                                ? new DcAddress
                                {
                                    Id = resp.LegalEntity.Address.AddressId,
                                    Address = resp.LegalEntity.Address.Residence,
                                    City = resp.LegalEntity.Address.City,
                                    State = resp.LegalEntity.Address.State,
                                    CountryIso2 = resp.LegalEntity.Address.Country
                                }
                                : null,
                        }
                        : null,
                    Address = resp.LegalEntity.Address != null
                        ? new DcAddress
                        {
                            Id = resp.LegalEntity.Address.AddressId,
                            Address = resp.LegalEntity.Address.Residence,
                            City = resp.LegalEntity.Address.City,
                            State = resp.LegalEntity.Address.State,
                            CountryIso2 = resp.LegalEntity.Address.Country
                        }
                        : null,
                };
            }
            else if (resp.Administration != null)
            {
                return new DcInstitutionInfo()
                {
                    ElectronicSubjectId = Guid.Parse(resp.Administration.ProfileSubjectId),
                    ElectronicSubjectName = resp.Administration.ProfileName,
                    UniqueSubjectIdentifier = resp.Administration.ProfileIdentifier,
                    DateCreated = resp.Administration.DateCreated.ToLocalDateTime(),
                    IsActivated = resp.Administration.IsActivated,
                    Email = resp.Administration.Email,
                    PhoneNumber = resp.Administration.Phone,
                    Name = resp.Administration.Name,
                    HeadInstitution = resp.Administration.Parent != null
                        ? new DcSubjectPublicInfo()
                        {
                            ElectronicSubjectId = Guid.Parse(resp.Administration.Parent.ProfileSubjectId),
                            ElectronicSubjectName = resp.Administration.Parent.ProfileName,
                            IsActivated = resp.Administration.Parent.IsActivated,
                            Email = resp.Administration.Parent.Email,
                            PhoneNumber = resp.Administration.Parent.Phone,
                        }
                        : null,
                    SubInstitutions = resp.Administration
                        .Children
                        .Select(e => new DcSubjectPublicInfo()
                        {
                            ElectronicSubjectId = Guid.Parse(e.ProfileSubjectId),
                            ElectronicSubjectName = e.ProfileName,
                            IsActivated = e.IsActivated,
                            Email = e.Email,
                            PhoneNumber = e.Phone,
                        })
                        .ToList(),
                    Address = resp.Administration.Address != null
                        ? new DcAddress
                        {
                            Id = resp.Administration.Address.AddressId,
                            Address = resp.Administration.Address.Residence,
                            City = resp.Administration.Address.City,
                            State = resp.Administration.Address.State,
                            CountryIso2 = resp.Administration.Address.Country
                        }
                        : null,
                };
            }

            return null;
        }
    }
}
