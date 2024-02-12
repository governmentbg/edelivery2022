namespace ED.Domain
{
    partial class TranslationsListQueryRepository : Repository, ITranslationsListQueryRepository
    {
        public TranslationsListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
