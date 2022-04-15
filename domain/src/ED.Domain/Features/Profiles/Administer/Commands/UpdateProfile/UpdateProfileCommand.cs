using MediatR;

namespace ED.Domain
{
    public record UpdateProfileCommand(
        int ProfileId,
        string Email,
        string Phone,
        string Residence,
        bool Sync,
        int ActionLoginId,
        string IP)
        : IRequest<UpdateProfileCommandResult>;

    public record UpdateProfileCommandResult(
        bool IsSuccessful,
        UpdateProfileCommandResultEnum Error);

    public enum UpdateProfileCommandResultEnum
    {
        Ok = 0,
        Unknown = 1,
        DuplicateEmail = 2,
    }
}
