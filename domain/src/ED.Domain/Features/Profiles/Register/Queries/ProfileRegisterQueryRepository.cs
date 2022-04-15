using Microsoft.IO;

namespace ED.Domain
{
    partial class ProfileRegisterQueryRepository : Repository, IProfileRegisterQueryRepository
    {
        private readonly BlobsServiceClient blobsServiceClient;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public ProfileRegisterQueryRepository(
            UnitOfWork unitOfWork,
            BlobsServiceClient blobsServiceClient,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager)
            : base(unitOfWork)
        {
            this.blobsServiceClient = blobsServiceClient;
            this.recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }
    }
}
