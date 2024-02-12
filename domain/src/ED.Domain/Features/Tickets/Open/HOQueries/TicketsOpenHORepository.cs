namespace ED.Domain
{
    partial class TicketsOpenHORepository : ITicketsOpenHORepository
    {
        private readonly IEncryptorFactory encryptorFactory;
        private readonly Keystore.Keystore.KeystoreClient keystoreClient;
        private readonly IProfilesService profilesService;
        private readonly ITicketsOpenQueryRepository ticketsOpenQueryRepository;

        public TicketsOpenHORepository(
            IEncryptorFactory encryptorFactory,
            Keystore.Keystore.KeystoreClient keystoreClient,
            IProfilesService profilesService,
            ITicketsOpenQueryRepository ticketsOpenQueryRepository)
        {
            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.profilesService = profilesService;
            this.ticketsOpenQueryRepository = ticketsOpenQueryRepository;
        }
    }
}
