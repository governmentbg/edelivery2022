namespace EDelivery.WebPortal.Models
{
    public class RegisteredSubjectsListModel : RegisteredSubjectsModel
    {
        public RegisteredSubjectsListModel(Enums.eRegisteredSubjectsType type)
        {
            this.Type = type;
            this.Items = new PagedList.PagedListLight<RegisteredSubjectsItemModel>(
                new System.Collections.Generic.List<RegisteredSubjectsItemModel>(),
                SystemConstants.LargePageSize,
                1,
                0);
        }

        public PagedList.PagedListLight<RegisteredSubjectsItemModel> Items { get; set; }

        public string Search { get; internal set; }
    }
}
