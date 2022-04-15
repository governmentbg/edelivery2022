using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : Repository, IAdminRegistrationsEditQueryRepository
    {
        private ILogger logger;
        private BlobsServiceClient blobsServiceClient;
        private RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public AdminRegistrationsEditQueryRepository(
            UnitOfWork unitOfWork,
            BlobsServiceClient blobsServiceClient,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            ILogger<AdminProfilesListQueryRepository> logger)
            : base(unitOfWork)
        {
            this.blobsServiceClient = blobsServiceClient;
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            this.logger = logger;
        }
    }
}
