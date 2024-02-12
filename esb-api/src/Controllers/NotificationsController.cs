using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ED.DomainServices.Esb.EsbTicket;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/t_v{version:apiVersion}/notifications")]
public class NotificationsController : ControllerBase
{
    /// <summary>
    /// Изпращане на нотификация по имейл, смс и/или вайбър
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendNotificationAsync(
        [FromServices] EsbTicketClient esbTicketClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] NotificationSendDO notification,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        _ = await esbTicketClient.SendNotificationAsync(
                new DomainServices.Esb.SendNotificationRequest
                {
                    LegalEntityRecipient = notification.LegalEntity != null
                        ? new DomainServices.Esb.SendNotificationRequest.Types.LegalEntityRecipient
                        {
                            Email = notification.LegalEntity.Email,
                            Phone = notification.LegalEntity.Phone,
                        }
                        : null,
                    IndividualRecipient = notification.Individual != null
                        ? new DomainServices.Esb.SendNotificationRequest.Types.IndividualRecipient
                        {
                            Email = notification.Individual.Email,
                            Phone = notification.Individual.Phone,
                        }
                        : null,
                    Email = notification.Email != null
                        ? new DomainServices.Esb.SendNotificationRequest.Types.Email
                        {
                            Subject = notification.Email.Subject,
                            Body = notification.Email.Body,
                        }
                        : null,
                    Sms = notification.Sms,
                    Viber = notification.Viber,
                    ProfileId = profileId,
                },
                cancellationToken: ct);

        return this.Accepted();
    }
}
