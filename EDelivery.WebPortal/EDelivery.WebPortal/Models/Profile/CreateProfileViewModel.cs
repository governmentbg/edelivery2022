using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class CreateProfileViewModel
    {
        [RequiredRes]
        public string FirstName { get; set; }
        [RequiredRes]
        public string MiddleName { get; set; }
        [RequiredRes]
        public string LastName { get; set; }
        [RequiredRes]
        public string EmailAddress { get; set; }
        [RequiredRes]
        public string PhoneNumber { get; set; }
        [RequiredRes]
        public string Address { get; set; }
        public bool IsEmailNotificationEnabled { get; set; }
        public bool IsPhoneNotificationEnabled { get; set; }
    }
}
