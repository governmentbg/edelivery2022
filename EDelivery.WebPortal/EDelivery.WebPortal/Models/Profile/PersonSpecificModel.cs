namespace EDelivery.WebPortal.Models
{
    public class PersonSpecificModel
    {
        public PersonSpecificModel()
        {
        }

        public PersonSpecificModel(
            string firstName,
            string middleName,
            string lastName,
            string egn)
        {
            this.FirstName = firstName;
            this.MiddleName = middleName;
            this.LastName = lastName;
            this.EGN = egn;
        }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string EGN { get; set; }
    }
}