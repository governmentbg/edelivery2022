namespace ED.Domain
{
    partial class JobsTranslationsListQueryRepository : Repository, IJobsTranslationsListQueryRepository
    {
        public JobsTranslationsListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
