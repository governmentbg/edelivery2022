namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : Repository, IIntegrationServiceMessagesOpenQueryRepository
    {
        private readonly BlobsServiceClient blobsServiceClient;
        private readonly IEncryptorFactory encryptorFactory;
        private readonly ED.Keystore.Keystore.KeystoreClient keystoreClient;
        private readonly IProfilesService profilesService;

        public IntegrationServiceMessagesOpenQueryRepository(
            IEncryptorFactory encryptorFactory,
            ED.Keystore.Keystore.KeystoreClient keystoreClient,
            IProfilesService profilesService,
            BlobsServiceClient blobsServiceClient,
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.profilesService = profilesService;
            this.blobsServiceClient = blobsServiceClient;
        }
    }
}
