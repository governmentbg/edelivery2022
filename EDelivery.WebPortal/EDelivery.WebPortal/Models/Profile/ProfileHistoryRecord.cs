using System;
using System.Linq;
using System.Resources;
using System.Xml.Linq;

namespace EDelivery.WebPortal.Models
{
    public class ProfileHistoryRecord
    {
        public class ProfileHistoryRecordDetails
        {
            public string UserName { get; set; }

            public Guid UserGuid { get; set; }

            public string Data { get; set; }
        }

        public ProfileHistoryRecord(
            ED.DomainServices.Profiles.GetHistoryResponse.Types.History history,
            ResourceManager resourceManager)
        {
            this.Date = history.ActionDate.ToLocalDateTime();
            this.LoginName = history.LoginName;
            this.AdminName = history.AdminName;
            this.Ip = history.Ip ?? string.Empty;
            this.Action = GetActionName(history.Action, resourceManager);
            this.Details = GetDetails(
                history.Action,
                ParseDetails(history.Details ?? string.Empty),
                resourceManager);
        }

        public DateTime Date { get; set; }

        public string LoginName { get; set; }

        public string AdminName { get; set; }

        public string UserName => LoginName ?? $"{AdminName ?? string.Empty}*";

        public string Action { get; set; }

        public string Ip { get; set; }

        public string Details { get; set; }

        private string GetActionName(
            ED.DomainServices.ProfileHistoryAction action,
            ResourceManager resourceManager)
        {
            string name = "HistoryAction" + action.ToString();

            return resourceManager.GetString(name);
        }

        private ProfileHistoryRecordDetails ParseDetails(string details)
        {
            if (string.IsNullOrEmpty(details))
            {
                return null;
            }

            ProfileHistoryRecordDetails result =
                new ProfileHistoryRecordDetails();

            XDocument doc = XDocument.Parse(details, LoadOptions.None);

            XElement userName = doc
                .Root.Elements()
                .SingleOrDefault(x => x.Name == "UserName");
            if (userName != null)
            {
                result.UserName = userName.Value;
            }

            XElement userGuid = doc
                .Root
                .Elements()
                .SingleOrDefault(x => x.Name == "UserId");
            if (userGuid != null)
            {
                result.UserGuid = Guid.Parse(userGuid.Value);
            }

            XElement data = doc
                .Root
                .Elements()
                .SingleOrDefault(x => x.Name == "ActionDetails");

            if (data != null)
            {
                result.Data = data.Value;
            }

            return result;
        }

        private string GetDetails(
            ED.DomainServices.ProfileHistoryAction action,
            ProfileHistoryRecordDetails details,
            ResourceManager resourceManager)
        {
            if (details == null)
            {
                return string.Empty;
            }

            string name = "HistoryActionDetails" + action.ToString();

            string value = resourceManager.GetString(name);
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string result =
                action == ED.DomainServices.ProfileHistoryAction.ProfileUpdated
                    ? string.Format(value, (details.Data ?? "").Replace("-", "{1}"))
                    : string.Format(value, details.UserName, details.Data, EDeliveryResources.ProfilePage.TextUpdatedTo);

            return result;
        }
    }
}
