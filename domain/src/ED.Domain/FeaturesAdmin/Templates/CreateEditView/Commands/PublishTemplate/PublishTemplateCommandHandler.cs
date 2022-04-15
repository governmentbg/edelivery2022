using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record PublishTemplateCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<PublishTemplateCommand, int>
    {
        public async Task<int> Handle(
            PublishTemplateCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.Publish(command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return template.TemplateId;
        }
    }
}
