using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ED.DomainServices.Esb.EsbTicket;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/t_v{version:apiVersion}/tickets")]
public class TicketsController : ControllerBase
{
    /// <summary>
    /// Изпращане на е-фиш
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendAsync(
        [FromServices] EsbTicketClient esbTicketClient,
        [FromServices] BlobsServiceClient blobsServiceClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] TicketSendDO ticket,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        // да се определи получателя -> ако е (any) ЮЛ и съществува, се връчва на ЮЛ
        // ако не, се връчва на профила на управителя ФЛ, с данни подадени от МВР
        // ако и управителя не съществува, то се регистрира пасивно
        DomainServices.Esb.GetOrCreateRecipientResponse recipientResponse =
            await esbTicketClient.GetOrCreateRecipientAsync(
                new DomainServices.Esb.GetOrCreateRecipientRequest
                {
                    LegalEntityRecipient = ticket.LegalEntity != null
                        ? new DomainServices.Esb.GetOrCreateRecipientRequest.Types.LegalEntityRecipient
                        {
                            Identifier = ticket.LegalEntity.Identifier,
                            Name = ticket.LegalEntity.Name,
                        }
                        : null,
                    IndividualRecipient = new DomainServices.Esb.GetOrCreateRecipientRequest.Types.IndividualRecipient
                    {
                        Identifier = ticket.Individual.Identifier,
                        FirstName = ticket.Individual.FirstName,
                        MiddleName = ticket.Individual.MiddleName,
                        LastName = ticket.Individual.LastName,
                        Email = ticket.Individual.Email,
                        Phone = ticket.Individual.Phone,
                    },
                    ActionLoginId = loginId,
                    Ip = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                },
                cancellationToken: ct);

        // файловете се качват в хранилището
        byte[] content = Convert.FromBase64String(ticket.Ticket.Document.Content);

        BlobsServiceClient.UploadBlobVO uploadedBlob =
            await blobsServiceClient.UploadProfileBlobAsync(
                ticket.Ticket.Document.Name,
                content.AsMemory(0, content.Length),
                profileId,
                loginId,
                Blobs.ProfileBlobAccessKeyType.Temporary,
                ticket.Ticket.Document.DocumentRegistrationNumber,
                ct);

        string blobValidation = this.ValidateUploadedBlob(ticket.Ticket.Document.Name, uploadedBlob);
        if (!string.IsNullOrEmpty(blobValidation))
        {
            return this.BadRequest(blobValidation);
        }

        // изпраща се съобщение-фиш
        DomainServices.Esb.SendTicketResponse ticketResponse =
            await esbTicketClient.SendTicketAsync(
                new DomainServices.Esb.SendTicketRequest
                {
                    IsRecipientIndividual = recipientResponse.IsIndividual,
                    RecipientProfileId = recipientResponse.ProfileId,
                    RecipientIdentifier = recipientResponse.Identifier,
                    SenderProfileId = profileId,
                    SenderLoginId = loginId,

                    NotificationEmail = ticket.Individual?.Email,
                    NotificationPhone = ticket.Individual?.Phone,

                    Subject = ticket.Ticket.Subject,
                    Body = ticket.Ticket.Body,
                    Type = (DomainServices.TicketType)(int)ticket.Ticket.Type, // todo
                    DocumentSeries = ticket.Ticket.DocumentSeries,
                    DocumentNumber = ticket.Ticket.DocumentNumber,
                    IssueDate = ticket.Ticket.IssueDate.ToTimestamp(),
                    VehicleNumber = ticket.Ticket.VehicleNumber,
                    ViolationDate = ticket.Ticket.ViolationDate.ToTimestamp(),
                    ViolatedProvision = ticket.Ticket.ViolatedProvision,
                    PenaltyProvision = ticket.Ticket.PenaltyProvision,
                    DueAmount = ticket.Ticket.DueAmount,
                    DiscountedPaymentAmount = ticket.Ticket.DiscountedPaymentAmount,
                    Iban = ticket.Ticket.IBAN,
                    Bic = ticket.Ticket.BIC,
                    PaymentReason = ticket.Ticket.PaymentReason,
                    Document = new DomainServices.Esb.SendTicketRequest.Types.Blob
                    {
                        FileName = ticket.Ticket.Document.Name,
                        HashAlgorithm = uploadedBlob.HashAlgorithm!,
                        Hash = uploadedBlob.Hash,
                        Size = Convert.ToUInt64(content.Length),
                        BlobId = uploadedBlob.BlobId!.Value,
                    },
                    DocumentIdentifier = ticket.Ticket.DocumentIdentifier,
                },
                cancellationToken: ct);

        return this.Ok(ticketResponse.TicketId);
    }

    /// <summary>
    /// Обновяване на статус на е-фиш
    /// </summary>
    /// <returns></returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo
    [HttpPost("{ticketId:int}/served")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ServedAsync(
        [FromServices] EsbTicketClient esbTicketClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int ticketId,
        [FromBody] TicketServeDO servedTicket,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        _ = await esbTicketClient.ServeTicketAsync(
            new DomainServices.Esb.ServeTicketRequest
            {
                TicketId = ticketId,
                ServeDate = servedTicket.ServeDate.ToTimestamp(),
                ActionLoginId = loginId,
            },
            cancellationToken: ct);

        return this.Ok();
    }

    /// <summary>
    /// Анулиране на е-фиш
    /// </summary>
    /// <returns></returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo
    [HttpPost("{ticketId:int}/annulled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AnnulledAsync(
        [FromServices] EsbTicketClient esbTicketClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int ticketId,
        [FromBody] TicketAnnulDO annulledTicket,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        _ = await esbTicketClient.AnnulTicketAsync(
            new DomainServices.Esb.AnnulTicketRequest
            {
                TicketId = ticketId,
                AnnulDate = annulledTicket.AnnulDate?.ToTimestamp(),
                AnnulmentReason = annulledTicket.AnnulmentReason,
                ActionLoginId = loginId,
            },
            cancellationToken: ct);

        return this.Ok();
    }

    /// <summary>
    /// Проверка за връчени фишове
    /// </summary>
    /// <returns></returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo
    [HttpPost("check")]
    [ProducesResponseType(typeof(TicketsCheckResponseDO[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckAsync(
        [FromServices] EsbTicketClient esbTicketClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] TicketsCheckRequestDO request,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.CheckTicketsResponse resp =
            await esbTicketClient.CheckTicketsAsync(
                new DomainServices.Esb.CheckTicketsRequest
                {
                    TicketIds = { request.TicketIds },
                    DeliveryStatus =
                        request.DeliveryStatus?.Adapt<DomainServices.TicketDeliveryStatus>()
                            ?? DomainServices.TicketDeliveryStatus.None,
                    From = request.From?.ToTimestamp() ?? null,
                    To = request.To?.ToTimestamp() ?? null,
                    ProfileId = profileId,
                },
                cancellationToken: ct);

        return this.Ok(
            resp.Result.ProjectToType<TicketsCheckResponseDO>().ToArray());
    }

    private string ValidateUploadedBlob(
        string fileName,
        BlobsServiceClient.UploadBlobVO blob)
    {
        if (blob.IsMalicious)
        {
            return $"File {fileName} is malicious";
        }

        if (!blob.BlobId.HasValue
            || string.IsNullOrEmpty(blob.Hash)
            || string.IsNullOrEmpty(blob.HashAlgorithm))
        {
            return $"File {fileName} could not be uploaded";
        }

        return string.Empty;
    }
}
