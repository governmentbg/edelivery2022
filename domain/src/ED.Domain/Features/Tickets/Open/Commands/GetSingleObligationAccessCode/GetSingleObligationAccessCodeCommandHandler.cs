using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record GetSingleObligationAccessCodeCommandHandler(
        ITicketsOpenQueryRepository TicketsOpenQueryRepository,
        EsbServiceClient EsbServiceClient)
        : IRequestHandler<GetSingleObligationAccessCodeCommand, GetSingleObligationAccessCodeCommandResult>
    {
        public async Task<GetSingleObligationAccessCodeCommandResult> Handle(
            GetSingleObligationAccessCodeCommand command,
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
                    headerRepresentedPersonID: null,
                    headerCorrespondentOID: null,
                    headerOperatorID: profileIdentifier,
                    ct);

            EsbServiceClient.SingleObligationDO singleObligationDO =
                await this.EsbServiceClient.GetObligationAccessCodeAsync(
                    token,
                    command.DocumentType,
                    command.DocumentIdentifier,
                    ct);


            return new GetSingleObligationAccessCodeCommandResult(
                singleObligationDO.AccessCode,
                singleObligationDO.NotFoundMessage);
        }
    }
}
