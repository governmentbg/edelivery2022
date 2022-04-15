namespace ED.Domain
{
    partial class TargetGroupNomsRepository : EntityNomsRepository<TargetGroup, ActiveEntityNomVO>, ITargetGroupNomsRepository
    {
        public TargetGroupNomsRepository(UnitOfWork unitOfWork)
            : base(
                  unitOfWork,
                  e => e.TargetGroupId,
                  e => e.Name,
                  e => new ActiveEntityNomVO(e.TargetGroupId, e.Name, e.ArchiveDate == null))
        {
        }
    }
}
