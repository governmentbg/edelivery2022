namespace ED.Domain
{
    partial class ProfileListQueryRepository : Repository, IProfileListQueryRepository
    {
        private readonly RegixServiceClient regixServiceClient;

        public ProfileListQueryRepository(
            UnitOfWork unitOfWork,
            RegixServiceClient client)
            : base(unitOfWork)
        {
            this.regixServiceClient = client;
        }
    }
}
