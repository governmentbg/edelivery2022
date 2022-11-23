using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record EditTemplateCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<EditTemplateCommand, int>
    {
        public async Task<int> Handle(
            EditTemplateCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.Update(
                command.Name,
                command.IdentityNumber,
                command.Category,
                command.Content,
                command.ResponseTemplateId,
                command.IsSystemTemplate,
                command.ReadLoginSecurityLevelId,
                command.WriteLoginSecurityLevelId);

            await this.UnitOfWork.SaveAsync(ct);

            return template.TemplateId;
        }
    }
}
