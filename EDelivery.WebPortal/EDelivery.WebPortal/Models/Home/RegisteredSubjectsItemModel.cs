namespace EDelivery.WebPortal.Models
{
    public class RegisteredSubjectsItemModel
    {
        public RegisteredSubjectsItemModel()
        {
        }

        public RegisteredSubjectsItemModel(
            ED.DomainServices.Profiles.GetTargetGroupProfilesResponse.Types.Profile profile)
        {
            this.Name = profile.Name;
            this.EIK = profile.Identifier;
        }

        public string Name { get; set; }

        public string EIK { get; set; }
    }
}