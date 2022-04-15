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
                command.Content,
                command.ResponseTemplateId,
                command.IsSystemTemplate,
                command.CreatedBy,
                command.ReadLoginSecurityLevelId,
                command.WriteLoginSecurityLevelId,
                command.BlobId,
                command.SenderDocumentField,
                command.RecipientDocumentField,
                command.SubjectDocumentField,
                command.DateSentDocumentField,
                command.DateReceivedDocumentField,
                command.SenderSignatureDocumentField,
                command.RecipientSignatureDocumentField);

            await this.TemplateAggregateRepository.AddAsync(template, ct);

            await this.UnitOfWork.SaveAsync(ct);

            return template.TemplateId;
        }
    }
}
