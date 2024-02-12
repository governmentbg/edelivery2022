using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record LoadMultipleObligationsCommandHandler(
        ITicketsOpenQueryRepository TicketsOpenQueryRepository,
        EsbServiceClient EsbServiceClient)
        : IRequestHandler<LoadMultipleObligationsCommand, LoadMultipleObligationsCommandResult>
    {
        public async Task<LoadMultipleObligationsCommandResult> Handle(
            LoadMultipleObligationsCommand command,
            CancellationToken ct)
        {
            string profileIdentifier =
                await this.TicketsOpenQueryRepository.GetProfileIdentifierAsync(
                    command.ProfileId,
                    ct);

            // todo: cache
            string token =
                await this.EsbServiceClient.GetTokenAsync(
                    EsbServiceClient.EsbScope.TicketScope,
                    headerRepresentedPersonID: $"PNOBG-{profileIdentifier}",
                    headerCorrespondentOID: null,
                    headerOperatorID: null,
                    ct);

            EsbServiceClient.MultipleObligationsDO multipleObligationsDO =
                await this.EsbServiceClient.LoadObligationsAsync(
                    token,
                    ct);

            return new LoadMultipleObligationsCommandResult(
                multipleObligationsDO.Count,
                multipleObligationsDO.NotFoundMessage);
        }
    }
}
