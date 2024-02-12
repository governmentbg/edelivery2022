using System;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Translations;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class TranslationService : Translations.Translation.TranslationBase
    {
        private readonly IServiceProvider serviceProvider;

        public TranslationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetTranslationsResponse> GetTranslations(
            GetTranslationsRequest request,
            ServerCallContext context)
        {
            ITranslationsListQueryRepository.GetMessageTranslationsVO[] translations =
                await this.serviceProvider
                    .GetRequiredService<ITranslationsListQueryRepository>()
                    .GetMessageTranslationsAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return new GetTranslationsResponse
            {
                Length = translations.Length,
                Result =
                {
                    translations.ProjectToType<GetTranslationsResponse.Types.Translation>()
                }
            };
        }

        public override async Task<GetTranslationResponse> GetTranslation(
            GetTranslationRequest request,
            ServerCallContext context)
        {
            ITranslationsListQueryRepository.GetMessageTranslationVO translation =
                await this.serviceProvider
                    .GetRequiredService<ITranslationsListQueryRepository>()
                    .GetMessageTranslationAsync(
                        request.MessageTranslationId,
                        context.CancellationToken);

            return new GetTranslationResponse
            {
                Translation = translation.Adapt<GetTranslationResponse.Types.Translation>()
            };
        }

        public override async Task<Empty> AddMessageTranslation(
            AddMessageTranslationRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateMessageTranslationCommand(
                        request.MessageId,
                        request.ProfileId,
                        request.SourceLanguage,
                        request.TargetLanguage,
                        request.LoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ArchiveMessageTranslation(
            ArchiveMessageTranslationRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new ArchiveMessageTranslationCommand(
                       request.MessageTranslationId,
                       request.LoginId),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<CheckExistingMessageTranslationResponse> CheckExistingMessageTranslation(
            CheckExistingMessageTranslationRequest request,
            ServerCallContext context)
        {
            bool isExisting =
                await this.serviceProvider
                    .GetRequiredService<ITranslationsCreateEditViewQueryRepository>()
                    .CheckExistingTranslationAsync(
                        request.MessageId,
                        request.ProfileId,
                        request.SourceLanguage,
                        request.TargetLanguage,
                        context.CancellationToken);

            return new CheckExistingMessageTranslationResponse
            {
                IsExisting = isExisting
            };
        }

        public override async Task<GetMessageTranslationsCountResponse> GetMessageTranslationsCount(
            GetMessageTranslationsCountRequest request,
            ServerCallContext context)
        {
            int count =
                await this.serviceProvider
                    .GetRequiredService<ITranslationsCreateEditViewQueryRepository>()
                    .GetMessageTranslationsCountAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return new GetMessageTranslationsCountResponse
            {
                Count = count
            };
        }

        public override async Task<GetArchivedMessageTranslationsCountResponse> GetArchivedMessageTranslationsCount(
            GetArchivedMessageTranslationsCountRequest request,
            ServerCallContext context)
        {
            int count =
                await this.serviceProvider
                    .GetRequiredService<ITranslationsCreateEditViewQueryRepository>()
                    .GetArchivedMessageTranslationsCountAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return new GetArchivedMessageTranslationsCountResponse
            {
                Count = count
            };
        }
    }
}
