using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateOrGetStatisticsCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<MessageStatistics> MessageStatisticsAggregateRepository,
        IEsbStatisticsListQueryRepository EsbStatisticsListQueryRepository)
        : IRequestHandler<CreateOrGetStatisticsCommand, CreateOrGetStatisticsCommandResult[]>
    {
        private readonly DateTime StatisticsBeginDate =
            new(2015, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public async Task<CreateOrGetStatisticsCommandResult[]> Handle(
            CreateOrGetStatisticsCommand command,
            CancellationToken ct)
        {
            DateTime beginMonth = this.GetFirstDayOfMonth(
                (await this.EsbStatisticsListQueryRepository.GetLastStatisticsAsync(ct))?.MonthDate.AddMonths(1)
                    ?? this.StatisticsBeginDate.AddMonths(-1));

            DateTime endMonth = this.GetFirstDayOfMonth(command.MonthDate?.Date ?? DateTime.Now.Date);

            DateTime nowMonth = this.GetFirstDayOfMonth(DateTime.Now.Date);

            await using ITransaction transaction =
               await this.UnitOfWork.BeginTransactionAsync(ct);

            while (beginMonth <= endMonth && beginMonth < nowMonth)
            {
                IEsbStatisticsListQueryRepository.GetMessagesCountVO messsagesCount =
                    await this.EsbStatisticsListQueryRepository.GetMessagesCountAsync(
                        beginMonth,
                        ct);

                MessageStatistics messageStatistics = new(
                    beginMonth,
                    messsagesCount.Sent,
                    messsagesCount.Received);

                await this.MessageStatisticsAggregateRepository.AddAsync(
                    messageStatistics,
                    ct);

                await this.UnitOfWork.SaveAsync(ct);

                beginMonth = beginMonth.AddMonths(1);
            }

            await transaction.CommitAsync(ct);

            if (command.MonthDate.HasValue)
            {
                DateTime commandMonth = 
                    this.GetFirstDayOfMonth(command.MonthDate.Value);

                if (commandMonth < nowMonth)
                {
                    IEsbStatisticsListQueryRepository.GetMonthStatisticsVO result =
                    (await this.EsbStatisticsListQueryRepository.GetMonthStatisticsAsync(
                        commandMonth,
                        ct))!;

                    return new CreateOrGetStatisticsCommandResult[]
                    {
                        new CreateOrGetStatisticsCommandResult(
                            result.Month,
                            command.Type == CreateOrGetStatisticsCommandType.Sent
                                ? result.MessagesSent
                                : result.MessagesReceived)
                    };
                }
                else if (commandMonth == nowMonth)
                {
                    IEsbStatisticsListQueryRepository.GetMessagesCountVO messsagesCount =
                    await this.EsbStatisticsListQueryRepository.GetMessagesCountAsync(
                        commandMonth,
                        ct);

                    return new CreateOrGetStatisticsCommandResult[]
                    {
                        new CreateOrGetStatisticsCommandResult(
                            commandMonth.ToString("yyyy-MM"),
                            command.Type == CreateOrGetStatisticsCommandType.Sent
                                ? messsagesCount.Sent
                                : messsagesCount.Received)
                    };
                }
                else
                {
                    return Array.Empty<CreateOrGetStatisticsCommandResult>();
                }
            }
            else
            {
                IEsbStatisticsListQueryRepository.GetAllStatisticsVO[] result =
                     await this.EsbStatisticsListQueryRepository.GetAllStatisticsAsync(ct);

                return result
                    .Select(e => new CreateOrGetStatisticsCommandResult(
                        e.Month,
                        command.Type == CreateOrGetStatisticsCommandType.Sent
                            ? e.MessagesSent
                            : e.MessagesReceived))
                    .ToArray();
            }
        }

        private DateTime GetFirstDayOfMonth(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }
    }
}
