using Microsoft.IO;

namespace ED.Domain
{
    partial class JobsMessagesOpenQueryRepository : Repository, IJobsMessagesOpenQueryRepository
    {
        private readonly BlobsServiceClient blobsServiceClient;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public JobsMessagesOpenQueryRepository(
            BlobsServiceClient blobsServiceClient,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            this.blobsServiceClient = blobsServiceClient;
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }
    }
}
