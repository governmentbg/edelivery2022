namespace ED.Domain
{
    partial class MessageOpenQueryRepository : Repository, IMessageOpenQueryRepository
    {
        private readonly EncryptorFactoryV1 encryptorFactoryV1;

        public MessageOpenQueryRepository(
            UnitOfWork unitOfWork,
            EncryptorFactoryV1 encryptorFactoryV1)
            : base(unitOfWork)
        {
            this.encryptorFactoryV1 = encryptorFactoryV1;
        }
    }
}
