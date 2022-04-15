namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : Repository, ICodeMessageOpenQueryRepository
    {
        private readonly EncryptorFactoryV1 encryptorFactoryV1;

        public CodeMessageOpenQueryRepository(
            EncryptorFactoryV1 encryptorFactoryV1,
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            this.encryptorFactoryV1 = encryptorFactoryV1;
        }
    }
}
