using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class AddPersonModel
    {
        public int ProfileId { get; set; }

        [RequiredRes]
        public string FirstName { get; set; }

        [RequiredRes]
        public string LastName { get; set; }

        [RequiredRes]
        public string EGN { get; set; }
    }
}