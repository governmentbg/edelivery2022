using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ED.DomainServices.Esb;
using Microsoft.AspNetCore.Mvc;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/stats")]
public class StatisticsController : ControllerBase
{
    /// <summary>
    /// Връща статистика за изпратените съобщения за период
    /// </summary>
    /// <param name="from">От дата</param>
    /// <param name="to">До дата</param>
    [HttpGet("messages/sent")]
    public async Task<MessageStatisticsDO> GetSentMessagesStatisticsAsync(
        [FromServices] EsbClient esbClient,
        [FromQuery, Required] DateTime from,
        [FromQuery, Required] DateTime to,
        CancellationToken ct)
    {
        GetSentMessageCountResponse resp =
            await esbClient.GetSentMessageCountAsync(
                new GetSentMessageCountRequest
                {
                    FromDate = from.ToTimestamp(),
                    ToDate = to.ToTimestamp(),
                },
                cancellationToken: ct);

        return new MessageStatisticsDO(resp.Value);
    }

    /// <summary>
    /// Връща статистика за получените съобщения за период
    /// </summary>
    /// <param name="from">От дата</param>
    /// <param name="to">До дата</param>
    [HttpGet("messages/received")]
    public async Task<MessageStatisticsDO> GetReceivedMessagesStatisticsAsync(
        [FromServices] EsbClient esbClient,
        [FromQuery, Required] DateTime from,
        [FromQuery, Required] DateTime to,
        CancellationToken ct)
    {
        GetReceivedMessageCountResponse resp =
            await esbClient.GetReceivedMessageCountAsync(
                new GetReceivedMessageCountRequest
                {
                    FromDate = from.ToTimestamp(),
                    ToDate = to.ToTimestamp(),
                },
                cancellationToken: ct);

        return new MessageStatisticsDO(resp.Value);
    }

    /// <summary>
    /// Връща статистика за изпратените съобщения по месеци
    /// </summary>
    /// <param name="month"></param>
    /// <returns></returns>
    [HttpGet("messages/sent-by-month")]
    public async Task<MessageMonthlyStatisticsDO[]> GetSentMessagesMonthlyStatisticsAsync(
        [FromServices] EsbClient esbClient,
        [FromQuery] DateTime? month,
        CancellationToken ct)
    {
        GetMessagesStatisticsResponse resp =
            await esbClient.GetMessageSentStatisticsAsync(
                new GetMessagesStatisticsRequest
                {
                    MontDate = month?.ToTimestamp()
                },
                cancellationToken: ct);

        return resp.Result.ProjectToType<MessageMonthlyStatisticsDO>().ToArray();
    }

    /// <summary>
    /// Връща статистика за получените съобщения по месеци
    /// </summary>
    /// <param name="month"></param>
    /// <returns></returns>
    [HttpGet("messages/received-by-month")]
    public async Task<MessageMonthlyStatisticsDO[]> GetReceivedMessagesMonthlyStatisticsAsync(
        [FromServices] EsbClient esbClient,
        [FromQuery] DateTime? month,
        CancellationToken ct)
    {
        GetMessagesStatisticsResponse resp =
            await esbClient.GetMessageReceivedStatisticsAsync(
                new GetMessagesStatisticsRequest
                {
                    MontDate = month?.ToTimestamp()
                },
                cancellationToken: ct);

        return resp.Result.ProjectToType<MessageMonthlyStatisticsDO>().ToArray();
    }
}
