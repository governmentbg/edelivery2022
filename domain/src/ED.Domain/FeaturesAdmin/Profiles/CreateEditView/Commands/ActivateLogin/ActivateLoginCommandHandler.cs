using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ActivateLoginCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository)
        : IRequestHandler<ActivateLoginCommand, Unit>
    {
        public async Task<Unit> Handle(
            ActivateLoginCommand command,
            CancellationToken ct)
        {
            Login login =
                await this.LoginAggregateRepository.FindAsync(
                    command.LoginId,
                    ct);

            login.Activate();

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
