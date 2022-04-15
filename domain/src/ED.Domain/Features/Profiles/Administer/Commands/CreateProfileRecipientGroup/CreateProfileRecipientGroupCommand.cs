using MediatR;

namespace ED.Domain
{
    public record CreateProfileRecipientGroupCommand(
        string Name,
        int ProfileId,
        int LoginId)
        : IRequest<CreateProfileRecipientGroupCommandResult>;

    public record CreateProfileRecipientGroupCommandResult(
        int RecipientGroupId,
        bool IsSuccessful,
        string Error);
}
