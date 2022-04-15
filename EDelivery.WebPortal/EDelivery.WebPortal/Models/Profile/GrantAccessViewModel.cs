using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models.Profile
{
    public class GrantAccessViewModel
    {
        public int ProfileId { get; set; }

        [RequiredRes]
        public string FirstName { get; set; }

        [RequiredRes]
        public string LastName { get; set; }

        [RequiredRes]
        public string Identifier { get; set; }
    }
}