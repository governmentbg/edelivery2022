using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record EsbOpenTicketCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IAggregateRepository<Ticket> TicketAggregateRepository,
        TimestampServiceClient TimestampServiceClient,
        IEsbMessagesOpenQueryRepository EsbMessageOpenQueryRepository,
        ITicketsOpenQueryRepository TicketsOpenQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<EsbOpenTicketCommand, EsbOpenTicketCommandResult?>
    {
        public async Task<EsbOpenTicketCommandResult?> Handle(
            EsbOpenTicketCommand command,
            CancellationToken ct)
        {
            DateTime now = DateTime.Now;

            Message message = await this.MessageAggregateRepository
                .FindAsync(command.MessageId, ct);

            Ticket ticket = await this.TicketAggregateRepository
                .FindAsync(command.MessageId, ct);

            string profileIdentifier = await this.TicketsOpenQueryRepository
                .GetProfileIdentifierAsync(
                    command.ProfileId,
                    ct);

            if (message.IsAlreadyOpen(command.ProfileId))
            {
                if (!ticket.IsStatusSeen())
                {
                    ticket.MarkStatusAsSeen(now, command.LoginId);
                    await this.UnitOfWork.SaveAsync(ct);
                }

                return null;
            }

            byte[]? recipientMessageSummary;
            string? recipientMessageSummaryXml = null;

            byte[] messageSummarySha256;
            if (message.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                throw new Exception("MessageSummary V1 is not supported");
            }
            else if (message.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                using MemoryStream memoryStream = new(
                    message.MessageSummary
                    ?? throw new Exception("MessageSummary should not be null"));

                memoryStream.Seek(0, SeekOrigin.Begin);

                XmlSerializer serializer = new(typeof(Message.MessageSummaryDO));
                using XmlReader reader = XmlReader.Create(memoryStream);

                Message.MessageSummaryDO messageSummaryDO =
                    (Message.MessageSummaryDO?)serializer.Deserialize(reader)
                        ?? throw new Exception("MessageSummaryXml should not be null");

                messageSummaryDO.DateReceived = now;
                messageSummaryDO.Recipients = messageSummaryDO.Recipients
                    .Where(e => e.ProfileId == command.ProfileId)
                    .ToArray();

                // TODO: if message are "transferred" from one profile to another
                if (!messageSummaryDO.Recipients.Any())
                {
                    string profileName =
                        await this.TicketsOpenQueryRepository.GetProfileNameAsync(
                            command.ProfileId,
                            ct);

                    messageSummaryDO.Recipients =
                        new Message.MessageSummaryDO.MessageSummaryVOProfile[]
                        {
                            new Message.MessageSummaryDO.MessageSummaryVOProfile(
                                command.ProfileId,
                                profileName),
                        };
                }

                using MemoryStream xmlMemoryStream = new();
                using XmlWriter xmlWriter = XmlWriter.Create(
                    xmlMemoryStream,
                    new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Encoding = Encoding.UTF8
                    });

                serializer.Serialize(xmlWriter, messageSummaryDO);

                recipientMessageSummaryXml =
                    Encoding.UTF8.GetString(xmlMemoryStream.ToArray());
                recipientMessageSummary = Encoding.UTF8.GetBytes(recipientMessageSummaryXml);

                xmlMemoryStream.Position = 0;
                messageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(xmlMemoryStream);
            }
            else
            {
                throw new Exception($"Unknown MessageSummaryVersion {message.MessageSummaryVersion}");
            }

            byte[] timestamp = await this.TimestampServiceClient.SubmitAsync(
                message.MessageId,
                EncryptionHelper.Sha256Algorithm,
                messageSummarySha256,
                ct);

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            message.UpdateAsOpen(
                command.ProfileId,
                command.LoginId,
                now,
                timestamp,
                recipientMessageSummary,
                recipientMessageSummaryXml);

            await this.UnitOfWork.SaveAsync(ct);

            ticket.InternalServe(now, command.LoginId);

            await this.UnitOfWork.SaveAsync(ct);

            DeliveredTicketQueueMessage deliveredTicketQueueMessage =
                new(
                    QueueMessageFeatures.Tickets,
                    this.GetIdentifierType(profileIdentifier),
                    profileIdentifier,
                    command.MessageId,
                    now);

            await this.QueueMessagesService.PostMessageAsync(
                deliveredTicketQueueMessage,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            IEsbMessagesOpenQueryRepository.GetOpenMessageInfoVO openMessageInfo =
                await this.EsbMessageOpenQueryRepository.GetOpenMessageInfoAsync(
                    command.MessageId,
                    command.ProfileId,
                    ct);

            return new(
                openMessageInfo.DateReceived,
                openMessageInfo.LoginId,
                openMessageInfo.LoginName);
        }

        private IdentifierType GetIdentifierType(string identifier)
        {
            if (ProfileValidationUtils.IsValidEGN(identifier))
            {
                return IdentifierType.EGN;
            }
            else if (ProfileValidationUtils.IsValidLNC(identifier))
            {
                return IdentifierType.LNCH;
            }
            else
            {
                return IdentifierType.Other;
            }
        }
    }
}
