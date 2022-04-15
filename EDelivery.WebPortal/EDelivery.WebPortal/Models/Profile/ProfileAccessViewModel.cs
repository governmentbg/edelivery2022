using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models
{
    public class ProfileAccessViewModel
    {
        public ProfileAccessViewModel(
            int profileId,
            ED.DomainServices.Profiles.GetLoginsResponse.Types.Login[] logins)
        {
            this.ProfileId = profileId;
            this.AccessList.AddRange(
                logins
                    .Select(e => new AccessListItem(e))
                    .ToList());
        }

        public int ProfileId { get; set; }

        public List<AccessListItem> AccessList { get; set; } =
            new List<AccessListItem>();
    }

    public class AccessListItem
    {
        public AccessListItem(
            ED.DomainServices.Profiles.GetLoginsResponse.Types.Login login)
        {
            this.LoginId = login.LoginId;
            this.IsDefault = login.IsDefault;
            this.LoginName = login.LoginName;
            this.AccessGrantedByName = login.AccessGrantedByLoginName;
            this.AccessGrantedDate = login.DateAccessGranted.ToLocalDateTime();
        }

        public int LoginId { get; set; }

        public bool IsDefault { get; set; }

        public string LoginName { get; set; }

        public string AccessGrantedByName { get; set; }

        public DateTime AccessGrantedDate { get; set; }
    }
}