using System;
using MediatR;

namespace ED.Domain
{
    public record CreateOrGetStatisticsCommand(
        DateTime? MonthDate,
        CreateOrGetStatisticsCommandType Type)
        : IRequest<CreateOrGetStatisticsCommandResult[]>;

    public enum CreateOrGetStatisticsCommandType
    {
        Sent = 0,
        Received = 1,
    }

    public record CreateOrGetStatisticsCommandResult(
        string Month,
        int Value);
}
