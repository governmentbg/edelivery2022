namespace ED.Domain
{
    partial class TemplateListQueryRepository : Repository, ITemplateListQueryRepository
    {
        public TemplateListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
