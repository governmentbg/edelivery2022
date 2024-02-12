using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using ED.DomainServices.Tickets;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Grpc;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class ShortsController : BaseController
    {
        private readonly Lazy<Ticket.TicketClient> ticketClient;

        public ShortsController()
        {
            this.ticketClient =
                new Lazy<Ticket.TicketClient>(
                    () => GrpcClientFactory.CreateTicketClient(), isThreadSafe: false);
        }

        [AllowAnonymous]
        [Route("o")]
        [HttpGet]
        public async Task<ActionResult> LoadObligation(string q)
        {
            string decoded = Utils.Utils.FromUrlSafeBase64(q);
            System.Collections.Specialized.NameValueCollection parameters =
                HttpUtility.ParseQueryString(decoded);

            int profileId = int.Parse(parameters["p"]);
            int ticketId = int.Parse(parameters["t"]);

            GetTicketTypeAndDocumentIdentifierResponse respTicket =
                await this.ticketClient.Value.GetTicketTypeAndDocumentIdentifierAsync(
                    new GetTicketTypeAndDocumentIdentifierRequest
                    {
                        MessageId = ticketId,
                    });

            if (string.IsNullOrEmpty(respTicket.DocumentIdentifier))
            {
                throw new ArgumentException(
                    "Invalid parameter",
                    nameof(GetTicketTypeAndDocumentIdentifierResponse.DocumentIdentifier));
            }

            string externalTicketType = this.GetExternalTicketType(respTicket.DocumentType);

            GetSingleObligationAccessCodeResponse respObligation =
                await this.ticketClient.Value.GetSingleObligationAccessCodeAsync(
                    new GetSingleObligationAccessCodeRequest
                    {
                        ProfileId = profileId,
                        DocumentIdentifier = respTicket.DocumentIdentifier,
                        DocumentType = externalTicketType,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ViewBag.AccessCode = respObligation.AccessCode;
            ViewBag.NotFoundMessage = respObligation.NotFoundMessage;

            return View();
        }

        private string GetExternalTicketType(string type)
        {
            if (type.ToLowerInvariant().Contains("фиш".ToLowerInvariant()))
            {
                return ExternalTicketType.EL_TICKET.ToString();
            }
            else if (type.ToLowerInvariant().Contains("наказателно".ToLowerInvariant()))
            {
                return ExternalTicketType.PENAL_DECREE.ToString();
            }

            throw new ArgumentException(
                "Invalid parameter",
                nameof(GetTicketTypeAndDocumentIdentifierResponse.DocumentType));
        }

        private enum ExternalTicketType
        {
            TICKET,
            EL_TICKET,
            PENAL_DECREE,
            EL_TICKET_KZ,
            PENAL_DECREE_KZ
        }
    }
}
