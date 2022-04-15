using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace ED.Domain
{
    partial class EsbMessagesOpenHORepository : IEsbMessagesOpenHORepository
    {
        // TODO: remove unused
        private readonly EncryptorFactoryV1 encryptorFactoryV1;
        private readonly IEncryptorFactory encryptorFactory;
        private readonly ED.Keystore.Keystore.KeystoreClient keystoreClient;
        private readonly IProfilesService profilesService;
        private readonly IEsbMessagesOpenQueryRepository esbMessageOpenQueryRepository;
        private readonly BlobsServiceClient blobsServiceClient;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;
        private readonly IOptions<DomainOptions> domainOptionsAccessor;
        private readonly IOptions<PdfOptions> pdfOptionsAccessor;

        public EsbMessagesOpenHORepository(
            EncryptorFactoryV1 encryptorFactoryV1,
            IEncryptorFactory encryptorFactory,
            ED.Keystore.Keystore.KeystoreClient keystoreClient,
            IProfilesService profilesService,
            IEsbMessagesOpenQueryRepository esbMessageOpenQueryRepository,
            BlobsServiceClient blobsServiceClient,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            IOptions<DomainOptions> domainOptionsAccessor,
            IOptions<PdfOptions> pdfOptionsAccessor)
        {
            this.encryptorFactoryV1 = encryptorFactoryV1;
            this.encryptorFactory = encryptorFactory;
            this.keystoreClient = keystoreClient;
            this.profilesService = profilesService;
            this.esbMessageOpenQueryRepository = esbMessageOpenQueryRepository;
            this.blobsServiceClient = blobsServiceClient;
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            this.domainOptionsAccessor = domainOptionsAccessor;
            this.pdfOptionsAccessor = pdfOptionsAccessor;
        }
    }
}
