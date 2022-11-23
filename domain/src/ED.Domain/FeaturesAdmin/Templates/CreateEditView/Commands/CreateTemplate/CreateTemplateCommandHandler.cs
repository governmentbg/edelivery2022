using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateTemplateCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<CreateTemplateCommand, int>
    {
        public async Task<int> Handle(
            CreateTemplateCommand command,
            CancellationToken ct)
        {
            Template template = new(
                command.Name,
                command.IdentityNumber,
                command.Category,
                command.Content,
                command.ResponseTemplateId,
                command.IsSystemTemplate,
                command.CreatedBy,
                command.ReadLoginSecurityLevelId,
                command.WriteLoginSecurityLevelId);

            await this.TemplateAggregateRepository.AddAsync(template, ct);

            await this.UnitOfWork.SaveAsync(ct);

            return template.TemplateId;
        }
    }
}
