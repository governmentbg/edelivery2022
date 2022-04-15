using System;
using MediatR;

namespace ED.Domain
{
    public record CreateRegixReportsAuditLogCommand(
        string Token,
        string Data,
        int LoginId,
        int ProfileId,
        DateTime DateCreated)
        : IRequest;
}
