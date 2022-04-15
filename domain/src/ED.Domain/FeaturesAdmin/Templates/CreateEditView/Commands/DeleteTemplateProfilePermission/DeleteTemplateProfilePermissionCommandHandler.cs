using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeleteTemplateProfilePermissionCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<DeleteTemplateProfilePermissionCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeleteTemplateProfilePermissionCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.DeleteProfilePermission(command.ProfileId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
