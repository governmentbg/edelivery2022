using System;
using System.Collections.Generic;
using System.Linq;
using EDelivery.Common.DataContracts;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static List<DcMessage> ToListOfDcMessage(
            ED.DomainServices.IntegrationService.OutboxResponse resp)
        {
            return resp
                 .Result
                 .Select(e => new DcMessage
                 {
                     Id = e.MessageId,
                     Title = e.Subject,
                     IsDraft = false,
                     DateCreated = e.DateCreated.ToLocalDateTime(),
                     DateSent = e.DateSent.ToLocalDateTime(),
                     DateReceived = e.DateReceived?.ToLocalDateTime(),
                     SenderProfile = new DcProfile
                     {
                         Id = e.SenderProfile.ProfileId,
                         ElectronicSubjectId = Guid.Parse(e.SenderProfile.ProfileSubjectId),
                         ElectronicSubjectName = e.SenderProfile.ProfileName
                     },
                     ReceiverProfile = new DcProfile
                     {
                         Id = e.RecipientProfile.ProfileId,
                         ElectronicSubjectId = Guid.Parse(e.RecipientProfile.ProfileSubjectId),
                         ElectronicSubjectName = e.RecipientProfile.ProfileName
                     },
                     SenderLogin = new DcLogin
                     {
                         Id = e.SenderLogin.LoginId,
                         ElectronicSubjectId = Guid.Parse(e.SenderLogin.LoginSubjectId),
                         ElectronicSubjectName = e.SenderLogin.LoginName
                     },
                     ReceiverLogin = e.RecipientLogin != null
                        ? new DcLogin
                        {
                            Id = e.RecipientLogin.LoginId,
                            ElectronicSubjectId = Guid.Parse(e.RecipientLogin.LoginSubjectId),
                            ElectronicSubjectName = e.RecipientLogin.LoginName
                        }
                        : null
                 })
                 .ToList();
        }
    }
}
