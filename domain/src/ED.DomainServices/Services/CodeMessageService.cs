using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.CodeMessages;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class CodeMessageService : CodeMessage.CodeMessageBase
    {
        private readonly IServiceProvider serviceProvider;

        public CodeMessageService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<Empty> Send(
            SendRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new SendCodeMessageCommand(
                        request.Identifier,
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.Phone,
                        request.Email,
                        request.SenderProfileId,
                        request.SenderLoginId,
                        request.TemplateId,
                        request.Subject,
                        request.ReferencedOrn,
                        request.AdditionalIdentifier,
                        request.Body,
                        request.MetaFields,
                        request.SenderLoginId,
                        request.BlobIds.ToArray()),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<OpenResponse> Open(
            OpenRequest request,
            ServerCallContext context)
        {
            OpenCodeMessageCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new OpenCodeMessageCommand(Guid.Parse(request.AccessCode)),
                    context.CancellationToken);

            return result.Adapt<OpenResponse>();
        }

        public override async Task<ReadResponse> Read(
            ReadRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenHORepository.GetAsRecipientVO message =
               await this.serviceProvider
                   .GetRequiredService<ICodeMessageOpenHORepository>()
                   .GetAsRecipientAsync(
                       request.MessageId,
                       context.CancellationToken);

            return new ReadResponse
            {
                Message = message.Adapt<ReadResponse.Types.Message>(),
            };
        }

        public override async Task<GetSenderProfileResponse> GetSenderProfile(
            GetSenderProfileRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenQueryRepository.GetSenderProfileVO senderProfile =
               await this.serviceProvider
                   .GetRequiredService<ICodeMessageOpenQueryRepository>()
                   .GetSenderProfileAsync(
                       request.AccessCode,
                       context.CancellationToken);

            return senderProfile.Adapt<GetSenderProfileResponse>();
        }

        public override async Task<GetTimestampResponse> GetTimestampNRD(
            GetTimestampRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenQueryRepository.GetTimestampNrdVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<ICodeMessageOpenQueryRepository>()
                    .GetTimestampNrdAsync(
                        request.AccessCode,
                        context.CancellationToken);

            return timestamp.Adapt<GetTimestampResponse>();
        }

        public override async Task<GetTimestampResponse> GetTimestampNRO(
            GetTimestampRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenQueryRepository.GetTimestampNroVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<ICodeMessageOpenQueryRepository>()
                    .GetTimestampNroAsync(
                        request.AccessCode,
                        context.CancellationToken);

            return timestamp.Adapt<GetTimestampResponse>();
        }

        public override async Task<GetBlobTimestampResponse> GetBlobTimestamp(
           GetBlobTimestampRequest request,
           ServerCallContext context)
        {
            ICodeMessageOpenQueryRepository.GetBlobTimestampVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<ICodeMessageOpenQueryRepository>()
                    .GetBlobTimestampAsync(
                        request.AccessCode,
                        request.BlobId,
                        context.CancellationToken);

            return timestamp.Adapt<GetBlobTimestampResponse>();
        }

        public override async Task<GetSummaryResponse> GetSummary(
            GetSummaryRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenQueryRepository.GetSummaryVO summary =
                await this.serviceProvider
                    .GetRequiredService<ICodeMessageOpenQueryRepository>()
                    .GetSummaryAsync(
                        request.AccessCode,
                        context.CancellationToken);

            return summary.Adapt<GetSummaryResponse>();
        }

        public override async Task<GetPdfAsRecipientResponse> GetPdfAsRecipient(
            GetPdfAsRecipientRequest request,
            ServerCallContext context)
        {
            ICodeMessageOpenHORepository.GetPdfAsRecipientVO pdf =
              await this.serviceProvider
                  .GetRequiredService<ICodeMessageOpenHORepository>()
                  .GetPdfAsRecipientAsync(
                      Guid.Parse(request.AccessCode),
                      context.CancellationToken);

            return pdf.Adapt<GetPdfAsRecipientResponse>();
        }
    }
}
