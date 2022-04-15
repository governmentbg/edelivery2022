using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeactivateLoginCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository)
        : IRequestHandler<DeactivateLoginCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeactivateLoginCommand command,
            CancellationToken ct)
        {
            Login login =
                await this.LoginAggregateRepository.FindAsync(
                    command.LoginId,
                    ct);

            login.Deactivate();

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
