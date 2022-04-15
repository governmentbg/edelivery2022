using System.Collections.Generic;

using EDelivery.WebPortal.Enums;

namespace EDelivery.WebPortal.Models
{
    public class BreadCrumb
    {
        public List<BreadCrumbLink> Links { get; set; }

        public eLeftMenu ELeftMenu { get; set; } = eLeftMenu.None;

        public string ProfileName { get; set; }

        public BreadCrumb(string profileName, string homeUrl, string homeName)
        {
            ProfileName = profileName;
            Links = new List<BreadCrumbLink>(10)
            {
                new BreadCrumbLink()
                {
                    LinkUrl = homeUrl,
                    LinkName = homeName
                }
            };
        }
    }

    public class BreadCrumbLink
    {
        public string LinkName { get; set; }

        public string LinkUrl { get; set; }
    }
}