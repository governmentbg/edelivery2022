namespace ED.Domain
{
    partial class LoginSecurityLevelNomsRepository : EntityNomsRepository<LoginSecurityLevel, EntityNomVO>, ILoginSecurityLevelNomsRepository
    {
        public LoginSecurityLevelNomsRepository(UnitOfWork unitOfWork)
            : base(
                  unitOfWork,
                  e => e.LoginSecurityLevelId,
                  e => e.Name,
                  e => new EntityNomVO(e.LoginSecurityLevelId, e.Name))
        {
        }
    }
}
