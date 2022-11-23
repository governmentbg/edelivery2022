using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository authorizationRepository;
        public AuthorizationService(IAuthorizationRepository authorizationRepository)
        {
            this.authorizationRepository = authorizationRepository;
        }

        public async Task<bool> HasProfileAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.ExistsLoginProfileAsync(
                loginId,
                profileId,
                ct);
        }

        public async Task<bool> IsReadonlyProfileAsync(
            int profileId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.IsProfileReadOnlyAsync(
                profileId,
                ct);
        }

        public async Task<bool> HasReadMessageAsRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct)
        {
            int? templateId =
                await this.authorizationRepository.GetTemplateIdAsync(
                    messageId,
                    ct)
                    ?? throw new Exception("Message without a template");

            if (templateId == Template.SystemForwardTemplateId)
            {
                // special case as it is currently impossible to give permissions
                // for the system template used when forwarding a message
                templateId =
                    await this.authorizationRepository.GetForwardedMessageTemplateId(
                        messageId,
                        ct)
                        ?? throw new Exception("Message without a template");
            }

            // TODO: this may be unnecessary if we make sure that
            // an exception will be thrown if trying to view
            // a message without having the key
            if (!(await this.authorizationRepository.HasMessageAccessKeyAsync(
                profileId,
                messageId,
                ct)))
            {
                return false;
            }

            if (!(await this.authorizationRepository.IsMessageRecipientAsync(
                profileId,
                messageId,
                ct)))
            {
                return false;
            }

            return await this.authorizationRepository.HasReadProfileMessageAccessAsync(
                profileId,
                loginId,
                templateId.Value,
                ct);
        }

        public async Task<bool> HasReadMessageAsSenderAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct)
        {
            int? templateId =
                await this.authorizationRepository.GetTemplateIdAsync(messageId, ct);

            if (templateId == null)
            {
                // TODO what should we do with messages without templates?
                throw new System.Exception("Message without a template");
            }

            // TODO: this may be unnecessary if we make sure that
            // an exception will be thrown if trying to view
            // a message without having the key
            if (!(await this.authorizationRepository.HasMessageAccessKeyAsync(
                profileId,
                messageId,
                ct)))
            {
                return false;
            }

            if (!(await this.authorizationRepository.IsMessageSenderAsync(
                profileId,
                messageId,
                ct)))
            {
                return false;
            }

            return await this.authorizationRepository.HasReadProfileMessageAccessAsync(
                profileId,
                loginId,
                templateId.Value,
                ct);
        }

        public async Task<bool> HasReadMessageAsSenderOrRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct)
        {
            int? templateId =
                await this.authorizationRepository.GetTemplateIdAsync(messageId, ct);

            if (templateId == null)
            {
                // TODO what should we do with messages without templates?
                throw new System.Exception("Message without a template");
            }

            // TODO: this may be unnecessary if we make sure that
            // an exception will be thrown if trying to view
            // a message without having the key
            if (!(await this.authorizationRepository.HasMessageAccessKeyAsync(
                profileId,
                messageId,
                ct)))
            {
                return false;
            }

            return await this.authorizationRepository.HasReadProfileMessageAccessAsync(
                profileId,
                loginId,
                templateId.Value,
                ct);
        }

        public async Task<bool> HasWriteMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasWriteProfileMessageAccessAsync(
                profileId,
                loginId,
                templateId,
                ct);
        }

        public async Task<bool> HasForwardMessageAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct)
        {
            int? templateId =
                await this.authorizationRepository.GetTemplateIdAsync(messageId, ct);

            if (templateId == null)
            {
                // TODO what should we do with messages without templates?
                throw new System.Exception("Message without a template");
            }

            return await this.authorizationRepository.HasWriteProfileMessageAccessAsync(
                profileId,
                loginId,
                templateId.Value,
                ct);
        }

        public async Task<bool> HasAdministerProfileAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasAdministerProfileAccessAsync(
                profileId,
                loginId,
                ct);
        }

        public async Task<bool> HasListProfileMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasListProfileMessageAccessAsync(
                profileId,
                loginId,
                ct);
        }

        public async Task<bool> HasAdministerProfileRecipientGroupAccessAsync(
            int profileId,
            int recipientGroupId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasAdministerProfileRecipientGroupAccessAsync(
                profileId,
                recipientGroupId,
                ct);
        }

        public async Task<bool> HasAccessTargetGroupSearchAsync(
            int profileId,
            int targetGroupId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasAccessTargetGroupSearchAsync(
                profileId,
                targetGroupId,
                ct);
        }

        public async Task<bool> HasWriteCodeMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasWriteProfileCodeMessageAccessAsync(
                profileId,
                loginId,
                ct);
        }

        public async Task<bool> HasMessageAccessKeyAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            return await this.authorizationRepository.HasMessageAccessKeyAsync(
                profileId,
                messageId,
                ct);
        }

        public async Task<bool> HasReadMessageThroughForwardingAsRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            int? forwardingMessageId,
            CancellationToken ct)
        {
            if (!forwardingMessageId.HasValue)
            {
                return false;
            }

            bool hasAccessToForwardingMessage = await this.HasReadMessageAsRecipientAccessAsync(
                   profileId,
                   loginId,
                   forwardingMessageId.Value,
                   ct);

            if (!hasAccessToForwardingMessage)
            {
                return false;
            }


            bool isForwardedMessage =
                await this.authorizationRepository.IsForwardedMessage(
                    messageId,
                    forwardingMessageId.Value,
                    ct);

            return isForwardedMessage;
        }
    }
}
