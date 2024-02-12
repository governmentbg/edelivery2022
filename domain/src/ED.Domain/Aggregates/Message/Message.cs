using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Message
    {
        public record MessageBlobDO(
            int BlobIndex,
            int ProfileId,
            int ProfileKeyId,
            int BlobId,
            byte[] EncryptedKey);

        public record MessageBlobAccesKeyDO(
            int MessageBlobId,
            int ProfileId,
            int ProfileKeyId,
            byte[] EncryptedKey);

        public record MessageAccessKeyDO(
            int ProfileId,
            int ProfileKeyId,
            byte[] EncryptedKey);

        [XmlType("Message")]
        public class MessageSummaryDO
        {
            [XmlType("Profile")]
            public class MessageSummaryVOProfile
            {
                public MessageSummaryVOProfile()
                {
                    this.Name = null!;
                }

                public MessageSummaryVOProfile(
                    int profileId,
                    string name)
                {
                    this.ProfileId = profileId;
                    this.Name = name;
                }

                public MessageSummaryVOProfile(
                    int profileId,
                    string name,
                    int sentViaLoginId,
                    int? sentViaOperatorLoginId)
                {
                    this.ProfileId = profileId;
                    this.Name = name;
                    this.SentViaLoginId = sentViaLoginId;
                    this.SentViaOperatorLoginId = sentViaOperatorLoginId;
                }

                public int ProfileId { get; set; }
                public string Name { get; set; }
                public int? SentViaLoginId { get; set; }

                public bool ShouldSerializeSentViaLoginId()
                {
                    return this.SentViaLoginId.HasValue;
                }

                public int? SentViaOperatorLoginId { get; set; }

                public bool ShouldSerializeSentViaOperatorLoginId()
                {
                    return this.SentViaOperatorLoginId.HasValue;
                }
            }

            public class MessageSummaryDOHashProperty
            {
                public MessageSummaryDOHashProperty()
                {
                    this.Hash = null!;
                    this.HashAlgorithm = null!;
                }

                public MessageSummaryDOHashProperty(
                    string hash,
                    string hashAlgorithm)
                {
                    this.Hash = hash;
                    this.HashAlgorithm = hashAlgorithm;
                }

                public string Hash { get; set; }
                public string HashAlgorithm { get; set; }
            }

            [XmlType("Attachment")]
            public class MessageSummaryDOAttachment
            {
                public MessageSummaryDOAttachment()
                {
                    this.Hash = null!;
                    this.HashAlgorithm = null!;
                    this.FileName = null!;
                }

                public MessageSummaryDOAttachment(
                    string hash,
                    string hashAlgorithm,
                    string fileName,
                    string documentRegistrationNumber)
                {
                    this.Hash = hash;
                    this.HashAlgorithm = hashAlgorithm;
                    this.FileName = fileName;
                    this.DocumentRegistrationNumber = documentRegistrationNumber;
                }

                public string Hash { get; set; }
                public string HashAlgorithm { get; set; }
                public string FileName { get; set; }

                [XmlElement("RegNum")]
                public string? DocumentRegistrationNumber { get; set; }
            }

            [XmlType("ForwardedMessage")]
            public class MessageSummaryDOForwardedMessage
            {
                public MessageSummaryDOForwardedMessage()
                {
                    this.Hash = null!;
                    this.HashAlgorithm = null!;
                }

                public MessageSummaryDOForwardedMessage(
                    int messageId,
                    string hash,
                    string hashAlgorithm)
                {
                    this.MessageId = messageId;
                    this.Hash = hash;
                    this.HashAlgorithm = hashAlgorithm;
                }

                public int MessageId { get; set; }
                public string Hash { get; set; }
                public string HashAlgorithm { get; set; }
            }

            public MessageSummaryDO()
            {
                this.Sender = null!;
                this.Recipients = null!;
                this.Subject = null!;
                this.Body = null!;
                this.MetaFields = null!;
                this.Attachments = null!;
            }

            public MessageSummaryDO(
                int messageId,
                string? rnu,
                MessageSummaryVOProfile sender,
                MessageSummaryVOProfile[] recipients,
                DateTime? dateSent,
                string subject,
                MessageSummaryDOHashProperty body,
                MessageSummaryDOHashProperty metaFields,
                int templateId,
                MessageSummaryDOAttachment[] attachments)
            {
                this.MessageId = messageId;
                this.Rnu = rnu;
                this.Sender = sender;
                this.Recipients = recipients;
                this.DateSent = dateSent;
                this.Subject = subject;
                this.Body = body;
                this.MetaFields = metaFields;
                this.TemplateId = templateId;
                this.Attachments = attachments;
            }

            public MessageSummaryDO(
                int messageId,
                string? rnu,
                MessageSummaryVOProfile sender,
                MessageSummaryVOProfile[] recipients,
                DateTime? dateSent,
                string subject,
                MessageSummaryDOHashProperty body,
                MessageSummaryDOHashProperty metaFields,
                int templateId,
                MessageSummaryDOAttachment[] attachments,
                MessageSummaryDOForwardedMessage? forwardedMessage)
            {
                this.MessageId = messageId;
                this.Rnu = rnu;
                this.Sender = sender;
                this.Recipients = recipients;
                this.DateSent = dateSent;
                this.Subject = subject;
                this.Body = body;
                this.MetaFields = metaFields;
                this.TemplateId = templateId;
                this.Attachments = attachments;
                this.ForwardedMessage = forwardedMessage;
            }

            public int MessageId { get; set; }
            public string? Rnu { get; set; }

            public bool ShouldSerializeReferencedRnu()
            {
                return this.Rnu != null;
            }

            public MessageSummaryVOProfile Sender { get; set; }
            public MessageSummaryVOProfile[] Recipients { get; set; }
            public DateTime? DateSent { get; set; }
            public DateTime? DateReceived { get; set; }

            public bool ShouldSerializeDateReceived()
            {
                return this.DateReceived.HasValue;
            }

            public string Subject { get; set; }
            public MessageSummaryDOHashProperty Body { get; set; }
            public MessageSummaryDOHashProperty MetaFields { get; set; }
            public int TemplateId { get; set; }
            public MessageSummaryDOAttachment[] Attachments { get; set; }
            public MessageSummaryDOForwardedMessage? ForwardedMessage { get; set; }

            public bool ShouldSerializeForwardedMessage()
            {
                return this.ForwardedMessage != null;
            }
        }

        // EF constructor
        private Message()
        {
            this.Subject = null!;
            this.Body = null!;

            this.MessageSummary = null!;
            this.TimeStampNRO = null!;
            this.IV = null!;
            this.RecipientsAsText = null!;

            this.Forwarded = null!;
            this.Template = null!;
            this.SenderProfile = null!;
        }

        public Message(
            int senderLoginId,
            int senderProfileId,
            int[] recipientProfileIds,
            string recipientsAsText,
            int templateId,
            string subject,
            string? rnu,
            byte[] body,
            string metaFields,
            int createdBy,
            byte[] iv,
            MessageAccessKeyDO[] keys,
            MessageBlobDO[] blobs)
        {
            this.SenderLoginId = senderLoginId;
            this.SenderProfileId = senderProfileId;
            this.RecipientsAsText = recipientsAsText;
            this.TemplateId = templateId;
            this.Subject = subject;
            this.Rnu = rnu;
            this.Body = body;
            this.MetaFields = metaFields;
            this.CreatedBy = createdBy;
            this.IV = iv;

            DateTime now = DateTime.Now;

            this.DateCreated = now;
            this.DateSent = now;
            this.ForwardStatusId = (byte)ForwardStatus.None;

            this.recipients.AddRange(
                recipientProfileIds
                    .Select(e => new MessageRecipient(e)));

            this.messageAccessKeys.AddRange(
                keys
                    .Select(e => new MessageAccessKey(
                        e.ProfileId,
                        e.ProfileKeyId,
                        e.EncryptedKey)));

            foreach (var blobGroup in blobs.GroupBy(e => e.BlobIndex))
            {
                this.messageBlobs.AddRange(
                    blobGroup
                        .GroupBy(e => e.BlobId)
                        .Select(e => new MessageBlob(
                            e.Key,
                            e
                                .Select(e => new MessageBlob.MessageBlobAccessKeyDO(
                                    e.ProfileId,
                                    e.ProfileKeyId,
                                    e.EncryptedKey))
                                .ToArray())));
            }

            this.MessageSummary = null!;
            this.TimeStampNRO = null!;

            this.Forwarded = null!;
            this.Template = null!;
            this.SenderProfile = null!;
        }

        public Message(
            int senderLoginId,
            int senderProfileId,
            int[] recipientProfileIds,
            string recipientsAsText,
            int templateId,
            string subject,
            string? rnu,
            byte[] body,
            string metaFields,
            int createdBy,
            int? sentViaLoginId,
            byte[] iv,
            MessageAccessKeyDO[] keys,
            MessageBlobDO[] blobs)
        {
            this.SenderLoginId = senderLoginId;
            this.SenderProfileId = senderProfileId;
            this.RecipientsAsText = recipientsAsText;
            this.TemplateId = templateId;
            this.Subject = subject;
            this.Rnu = rnu;
            this.Body = body;
            this.MetaFields = metaFields;
            this.CreatedBy = createdBy;
            this.SentViaLoginId = sentViaLoginId;
            this.IV = iv;

            DateTime now = DateTime.Now;

            this.DateCreated = now;
            this.DateSent = now;
            this.ForwardStatusId = (byte)ForwardStatus.None;

            this.recipients.AddRange(
                recipientProfileIds
                    .Select(e => new MessageRecipient(e)));

            this.messageAccessKeys.AddRange(
                keys
                    .Select(e => new MessageAccessKey(
                        e.ProfileId,
                        e.ProfileKeyId,
                        e.EncryptedKey)));

            foreach (var blobGroup in blobs.GroupBy(e => e.BlobIndex))
            {
                this.messageBlobs.AddRange(
                    blobGroup
                        .GroupBy(e => e.BlobId)
                        .Select(e => new MessageBlob(
                            e.Key,
                            e
                                .Select(e => new MessageBlob.MessageBlobAccessKeyDO(
                                    e.ProfileId,
                                    e.ProfileKeyId,
                                    e.EncryptedKey))
                                .ToArray())));
            }

            this.MessageSummary = null!;
            this.TimeStampNRO = null!;

            this.Forwarded = null!;
            this.Template = null!;
            this.SenderProfile = null!;
        }

        public Message(
            int senderLoginId,
            int senderProfileId,
            int recipientProfileId,
            Guid accessCode,
            string recipientFirstName,
            string recipientMiddleName,
            string recipientLastName,
            string recipientPhone,
            string recipientEmail,
            int templateId,
            string subject,
            string? rnu,
            byte[] body,
            string metaFields,
            int createdBy,
            byte[] iv,
            MessageAccessKeyDO[] keys,
            MessageBlobDO[] blobs)
        {
            this.SenderLoginId = senderLoginId;
            this.SenderProfileId = senderProfileId;
            this.RecipientsAsText =
                $"{recipientFirstName} {recipientMiddleName} {recipientLastName}";
            this.TemplateId = templateId;
            this.Subject = subject;
            this.Rnu = rnu;
            this.Body = body;
            this.MetaFields = metaFields;
            this.CreatedBy = createdBy;
            this.IV = iv;

            DateTime now = DateTime.Now;

            this.DateCreated = now;
            this.DateSent = now;
            this.ForwardStatusId = (byte)ForwardStatus.None;

            this.recipients.Add(new MessageRecipient(recipientProfileId));

            this.messageAccessKeys.AddRange(
                keys
                    .Select(e => new MessageAccessKey(
                        e.ProfileId,
                        e.ProfileKeyId,
                        e.EncryptedKey)));

            foreach (var blobGroup in blobs.GroupBy(e => e.BlobIndex))
            {
                this.messageBlobs.AddRange(
                    blobGroup
                        .GroupBy(e => e.BlobId)
                        .Select(e => new MessageBlob(
                            e.Key,
                            e
                                .Select(e => new MessageBlob.MessageBlobAccessKeyDO(
                                    e.ProfileId,
                                    e.ProfileKeyId,
                                    e.EncryptedKey))
                                .ToArray())));
            }

            this.AccessCode = new(
                accessCode,
                recipientFirstName,
                recipientMiddleName,
                recipientLastName,
                recipientPhone,
                recipientEmail);

            this.MessageSummary = null!;
            this.TimeStampNRO = null!;

            this.Forwarded = null!;
            this.Template = null!;
            this.SenderProfile = null!;
        }

        public int MessageId { get; set; }

        public int SenderProfileId { get; set; }

        public int? SenderLoginId { get; set; }

        public string Subject { get; set; }

        public byte[]? Body { get; set; }

        public byte[]? MessageSummary { get; set; }

        public MessageSummaryVersion? MessageSummaryVersion { get; set; }

        public DateTime? DateSent { get; set; }

        public byte[]? TimeStampNRO { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateCreated { get; set; }

        public int? SentViaLoginId { get; set; }

        public string? SubjectExtended { get; set; }

        public ForwardStatus ForwardStatusId { get; set; }

        public byte[] IV { get; set; }

        public string? MessageSummaryXml { get; set; }

        public string? MetaFields { get; set; }

        public int? TemplateId { get; set; }

        public string RecipientsAsText { get; set; }

        public int? MessagePdfBlobId { get; set; }

        public string? Rnu { get; set; }

        public Template Template { get; set; }

        public Login? SenderLogin { get; set; }

        public Profile SenderProfile { get; set; }

        public ForwardedMessage Forwarded { get; set; }

        private List<MessageBlob> messageBlobs = new();

        public IReadOnlyCollection<MessageBlob> MessageBlobs =>
            this.messageBlobs.AsReadOnly();

        private List<MessageAccessKey> messageAccessKeys = new();

        public IReadOnlyCollection<MessageAccessKey> MessageAccessKeys =>
            this.messageAccessKeys.AsReadOnly();

        private List<MessageRecipient> recipients = new();

        public IReadOnlyCollection<MessageRecipient> Recipients =>
            this.recipients.AsReadOnly();

        public MessagesAccessCode? AccessCode { get; set; }

        public void UpdateExtendedSubject(
            string messageSummaryXml,
            byte[] messageSummary,
            byte[] sendTimestamp)
        {
            this.SubjectExtended = $"{this.MessageId}/{this.DateSent!.Value:dd'.'MM'.'yyyy} - {this.Subject}";
            this.MessageSummaryXml = messageSummaryXml;
            this.MessageSummary = messageSummary;
            this.MessageSummaryVersion = ED.Domain.MessageSummaryVersion.V2;
            this.TimeStampNRO = sendTimestamp;
        }

        public void AddRecipients(
            MessageAccessKeyDO[] messageAccessKeys,
            MessageBlobAccesKeyDO[] messageBlobsAccessKeys)
        {
            this.messageAccessKeys.AddRange(
                messageAccessKeys.Select(e => new MessageAccessKey(
                    e.ProfileId,
                    e.ProfileKeyId,
                    e.EncryptedKey)));

            foreach (MessageBlob messageBlob in this.messageBlobs)
            {
                MessageBlob.MessageBlobAccessKeyDO[] messageBlobAccessKeys =
                    messageBlobsAccessKeys
                        .Where(e => e.MessageBlobId == messageBlob.MessageBlobId)
                        .Select(e => new MessageBlob.MessageBlobAccessKeyDO(
                            e.ProfileId,
                            e.ProfileKeyId,
                            e.EncryptedKey))
                        .ToArray();


                messageBlob.AddRecipients(messageBlobAccessKeys);
            }
        }

        public bool IsAlreadyOpen(int profileId)
        {
            return this.Recipients
                .First(e => e.ProfileId == profileId)
                .DateReceived
                .HasValue;
        }

        public void UpdateAsOpen(
            int profileId,
            int loginId,
            DateTime dateReceived,
            byte[] timestamp,
            byte[]? messageSummary,
            string? messageSummaryXml)
        {
            MessageRecipient recipient =
                this.Recipients.First(e => e.ProfileId == profileId);

            recipient.UpdateAsOpen(
                loginId,
                dateReceived,
                timestamp,
                messageSummary,
                messageSummaryXml);
        }

        public void UpdateRecipientMessagePdfBlob(int profileId, int? blobId)
        {
            MessageRecipient match =
                this.recipients.Single(e => e.ProfileId == profileId);

            match.UpdateMessagePdfBlob(blobId);
        }

        public void AddForwardedMessage(
            int forwardedMessageId,
            string forwardedMessageSubject,
            int forwardedMessageSenderProfileId,
            string forwardedMessageSenderProfileName,
            Guid forwardedMessageSenderProfileSubjectId,
            string forwardedMessageSenderLoginName)
        {
            this.Forwarded = new ForwardedMessage(
                forwardedMessageId,
                forwardedMessageSubject,
                forwardedMessageSenderProfileId,
                forwardedMessageSenderProfileName,
                forwardedMessageSenderProfileSubjectId,
                forwardedMessageSenderLoginName);
        }
    }

    class MessageMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Messages";

            var builder = modelBuilder.Entity<Message>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.MessageId);
            builder.Property(e => e.MessageId).ValueGeneratedOnAdd();

            builder.Property(e => e.MessageSummaryXml).HasColumnType("xml");

            builder.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .IsRequired(false);

            builder.HasOne(e => e.SenderLogin)
                .WithMany()
                .HasForeignKey(e => e.SenderLoginId)
                .IsRequired(false);

            builder.HasOne(e => e.SenderProfile)
                .WithMany()
                .HasForeignKey(e => e.SenderProfileId);

            builder.HasOne(e => e.Forwarded)
                .WithOne()
                .HasForeignKey<ForwardedMessage>(e => e.MessageId);

            builder.HasMany(e => e.MessageBlobs)
                .WithOne()
                .HasForeignKey(e => e.MessageId);

            builder.HasMany(e => e.MessageAccessKeys)
                .WithOne()
                .HasForeignKey(e => e.MessageId);

            builder.HasMany(e => e.Recipients)
                .WithOne()
                .HasForeignKey(e => e.MessageId);

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(Message.MessagePdfBlobId))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.AccessCode)
                .WithOne()
                .HasPrincipalKey<Message>(e => e.MessageId)
                .HasForeignKey<MessagesAccessCode>(e => e.MessageId)
                .IsRequired(false);

            builder.HasOne<Ticket>()
                .WithOne()
                .HasForeignKey<Ticket>(e => e.MessageId);
        }
    }
}
