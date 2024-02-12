namespace ED.Domain
{
    partial class JobsMessagesOpenHORepository : IJobsMessagesOpenHORepository
    {
        private readonly IEncryptorFactory encryptorFactory;
        private readonly ED.Keystore.Keystore.KeystoreClient keystoreClient;
        private readonly IProfilesService profilesService;
        private readonly IJobsMessagesOpenQueryRepository jobsMessagesOpenQueryRepository;

        public JobsMessagesOpenHORepository(
            IEncryptorFactory encryptorFactory,
            ED.Keystore.Keystore.KeystoreClient keystoreClient,
            IProfilesService profilesService,
            IJobsMessagesOpenQueryRepository jobsMessagesOpenQueryRepository)
        {
            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.profilesService = profilesService;
            this.jobsMessagesOpenQueryRepository = jobsMessagesOpenQueryRepository;
        }
    }
}
