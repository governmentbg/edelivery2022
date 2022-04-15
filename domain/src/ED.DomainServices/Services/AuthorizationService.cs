using System;
using System.Threading.Tasks;
using ED.Domain;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class AuthorizationService : Authorization.AuthorizationBase
    {
        private readonly IServiceProvider serviceProvider;
        public AuthorizationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<HasAccessResponse> HasProfileAccess(
            HasProfileAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<IsReadonlyProfileResponse> IsReadonlyProfile(
            IsReadonlyProfileRequest request,
            ServerCallContext context)
        {
            bool isReadonly = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .IsReadonlyProfileAsync(
                    request.ProfileId,
                    context.CancellationToken);

            return new IsReadonlyProfileResponse
            {
                IsReadonly = isReadonly,
            };
        }

        public override async Task<HasAccessResponse> HasReadMessageAsRecipientAccess(
            HasReadMessageAsRecipientAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasReadMessageAsRecipientAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    request.MessageId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasReadMessageAsSenderAccess(
            HasReadMessageAsSenderAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasReadMessageAsSenderAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    request.MessageId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public async override Task<HasAccessResponse> HasReadMessageAsSenderOrRecipientAccess(
            HasReadMessageAsSenderOrRecipientAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasReadMessageAsSenderOrRecipientAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    request.MessageId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasWriteMessageAccess(
            HasWriteMessageAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasWriteMessageAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    request.TemplateId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasForwardMessageAccess(
            HasForwardMessageAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasForwardMessageAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    request.MessageId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasAdministerProfileAccess(
            HasAdministerProfileAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasAdministerProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasListProfileMessageAccess(
            HasListProfileMessageAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasListProfileMessageAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasAdministerProfileRecipientGroupAccess(
            HasAdministerProfileRecipientGroupAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasAdministerProfileRecipientGroupAccessAsync(
                    request.ProfileId,
                    request.RecipientGroupId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasAccessTargetGroupSearch(
            HasAccessTargetGroupSearchRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasAccessTargetGroupSearchAsync(
                    request.ProfileId,
                    request.TargetGroupId,
                    context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }

        public override async Task<HasAccessResponse> HasWriteCodeMessageAccess(
            HasWriteCodeMessageAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess = await this.serviceProvider
               .GetRequiredService<IAuthorizationService>()
               .HasWriteCodeMessageAccessAsync(
                   request.ProfileId,
                   request.LoginId,
                   context.CancellationToken);

            return new HasAccessResponse
            {
                HasAccess = hasAccess,
            };
        }
    }
}
