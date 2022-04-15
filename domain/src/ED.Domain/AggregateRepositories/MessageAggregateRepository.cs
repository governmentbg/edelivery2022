using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal class MessageAggregateRepository : AggregateRepository<Message>
    {
        public MessageAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        protected override Func<IQueryable<Message>, IQueryable<Message>>[] Includes =>
            new Func<IQueryable<Message>, IQueryable<Message>>[]
            {
                (q) => q
                    .Include(e => e.SenderProfile) // TODO: check
                    .Include(e => e.SenderLogin) // TODO: check
                    .Include(e => e.Recipients)
                    .Include(e => e.Forwarded)
                    .Include(e => e.MessageAccessKeys)
                    .Include(e => e.MessageBlobs)
                        .ThenInclude(m => m.MessageBlobAccessKeys) // TODO: check
                    .Include(e => e.AccessCode)
            };

        public async Task<Message?> FindByAccessCodeAsync(
            Guid accessCode,
            CancellationToken ct)
        {
            return await this.FindEntityOrDefaultAsync(
                e => e.AccessCode != null
                    && e.AccessCode.AccessCode == accessCode,
                ct);
        }
    }
}
