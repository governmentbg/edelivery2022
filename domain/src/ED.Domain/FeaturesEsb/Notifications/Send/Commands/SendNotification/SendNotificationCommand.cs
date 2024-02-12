using MediatR;

namespace ED.Domain
{
    public record SendNotificationCommand(
        SendNotificationCommandLegalEntity? LegalEntity,
        SendNotificationCommandIndividual? Individual,
        SendNotificationCommandEmail? Email,
        string? Sms,
        string? Viber,
        int ProfileId)
        : IRequest;

    public record SendNotificationCommandLegalEntity(
        string? Email,
        string? Phone);

    public record SendNotificationCommandIndividual(
        string? Email,
        string? Phone);

    public record SendNotificationCommandEmail(
        string Subject,
        string Body);
}
