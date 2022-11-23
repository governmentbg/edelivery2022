using System;
using System.Collections.Generic;
using System.Linq;

using EDelivery.WebPortal.Models.Templates.Components;

namespace EDelivery.WebPortal.Models.Messages
{
    public class ReadMessageViewModel
    {
        public class ReadMessageViewModelProfile
        {
            public ReadMessageViewModelProfile(
                ED.DomainServices.Messages.ReadResponse.Types.MessageProfile messageProfile)
            {
                this.ProfileId = messageProfile.ProfileId;
                this.Type = messageProfile.Type;
                this.Name = messageProfile.Name;
                this.IsReadOnly = messageProfile.IsReadOnly;
                this.LoginName = messageProfile.LoginName;
            }

            public ReadMessageViewModelProfile(
                ED.DomainServices.Messages.ViewResponse.Types.MessageProfile messageProfile)
            {
                this.ProfileId = messageProfile.ProfileId;
                this.Name = messageProfile.Name;
            }

            public int ProfileId { get; set; }

            public ED.DomainServices.ProfileType Type { get; set; }

            public string Name { get; set; }

            public bool IsReadOnly { get; set; }

            public string LoginName { get; set; }
        }

        public class ReadMessageViewModelBlob
        {
            public ReadMessageViewModelBlob(
                ED.DomainServices.Messages.ReadResponse.Types.MessageBlob blob)
            {
                this.BlobId = blob.BlobId;
                this.FileName = blob.FileName;
                this.Hash = blob.Hash;
                this.Size = blob.Size;
                this.DocumentRegistrationNumber = blob.DocumentRegistrationNumber;
                this.Status = blob.Status;
                this.IsMalicious = blob.IsMalicious;

                this.Signatures =
                    blob.Signatures
                        .OrderBy(s => s.IsTimestamp)
                        .Select(s => new ReadMessageViewModelBlobSignature(s))
                        .ToList();
            }

            public ReadMessageViewModelBlob(
                ED.DomainServices.Messages.ViewResponse.Types.MessageBlob blob)
            {
                this.BlobId = blob.BlobId;
                this.FileName = blob.FileName;
                this.Hash = blob.Hash;
                this.Size = blob.Size;
                this.DocumentRegistrationNumber = blob.DocumentRegistrationNumber;
                this.Status = blob.Status;
                this.IsMalicious = blob.IsMalicious;

                this.Signatures =
                    blob.Signatures
                        .OrderBy(s => s.IsTimestamp)
                        .Select(s => new ReadMessageViewModelBlobSignature(s))
                        .ToList();
            }

            public int BlobId { get; set; }

            public string FileName { get; set; }

            public string Hash { get; set; }

            public long? Size { get; set; }

            public string DocumentRegistrationNumber { get; set; }

            public ED.DomainServices.MalwareScanResultStatus Status { get; set; }

            public bool? IsMalicious { get; set; }

            public List<ReadMessageViewModelBlobSignature> Signatures { get; set; }
        }

        public class ReadMessageViewModelBlobSignature
        {
            public ReadMessageViewModelBlobSignature(
                ED.DomainServices.Messages.ReadResponse.Types.MessageBlobSignature blobSignature)
            {
                this.CoversDocument = blobSignature.CoversDocument;
                this.IsTimestamp = blobSignature.IsTimestamp;
                this.SignDate = blobSignature.SignDate.ToLocalDateTime();
                this.ValidAtTimeOfSigning = blobSignature.ValidAtTimeOfSigning;
                this.Issuer = blobSignature.Issuer;
                this.Subject = blobSignature.Subject;
                this.ValidFrom = blobSignature.ValidFrom.ToLocalDateTime();
                this.ValidTo = blobSignature.ValidTo.ToLocalDateTime();
            }

            public ReadMessageViewModelBlobSignature(
                ED.DomainServices.Messages.ViewResponse.Types.MessageBlobSignature blobSignature)
            {
                this.CoversDocument = blobSignature.CoversDocument;
                this.IsTimestamp = blobSignature.IsTimestamp;
                this.SignDate = blobSignature.SignDate.ToLocalDateTime();
                this.ValidAtTimeOfSigning = blobSignature.ValidAtTimeOfSigning;
                this.Issuer = blobSignature.Issuer;
                this.Subject = blobSignature.Subject;
                this.ValidFrom = blobSignature.ValidFrom.ToLocalDateTime();
                this.ValidTo = blobSignature.ValidTo.ToLocalDateTime();
            }

            public bool CoversDocument { get; set; }

            public bool IsTimestamp { get; set; }

            public DateTime SignDate { get; set; }

            public bool ValidAtTimeOfSigning { get; set; }

            public string Issuer { get; set; }

            public string Subject { get; set; }

            public DateTime ValidFrom { get; set; }

            public DateTime ValidTo { get; set; }
        }

        public ReadMessageViewModel(
            ED.DomainServices.Messages.ReadResponse.Types.Message message)
        {
            this.MessageId = message.MessageId;
            this.DateSent = message.DateSent.ToLocalDateTime();
            this.DateReceived = message.DateReceived?.ToLocalDateTime();
            this.Sender = new ReadMessageViewModelProfile(message.Sender);
            this.Recipient = new ReadMessageViewModelProfile(message.Recipient);
            this.TemplateId = message.TemplateId;
            this.Subject = message.Subject;
            this.Rnu = message.Rnu;
            this.Body = message.Body;
            this.ForwardStatusId = message.ForwardStatusId;
            this.TemplateName = message.TemplateName;
            this.Blobs = message.Blobs
                .Select(e => new ReadMessageViewModelBlob(e))
                .ToList();
        }

        public ReadMessageViewModel(
            ED.DomainServices.Messages.ReadResponse.Types.ForwardedMessage message)
        {
            this.MessageId = message.MessageId;
            this.DateSent = message.DateSent.ToLocalDateTime();
            this.DateReceived = null;
            this.Sender = new ReadMessageViewModelProfile(message.Sender);
            this.Recipient = null;
            this.TemplateId = message.TemplateId;
            this.Subject = message.Subject;
            this.Rnu = message.Rnu;
            this.Body = message.Body;
            this.ForwardStatusId = ED.DomainServices.ForwardStatus.None;
            this.TemplateName = message.TemplateName;
            this.Blobs = message.Blobs
                .Select(e => new ReadMessageViewModelBlob(e))
                .ToList();
        }

        public ReadMessageViewModel(
            ED.DomainServices.Messages.ReadResponse.Types.Message message,
            ED.DomainServices.Messages.ReadResponse.Types.ForwardedMessage forwardedMessage)
            : this(message)
        {
            if (forwardedMessage != null)
            {
                this.ForwardedMessage =
                    new ReadMessageViewModel(forwardedMessage);
            }
        }

        public ReadMessageViewModel(
            ED.DomainServices.Messages.ViewResponse.Types.ForwardedMessage forwardedMessage)
        {
            this.MessageId = forwardedMessage.MessageId;
            this.DateSent = forwardedMessage.DateSent.ToLocalDateTime();
            this.DateReceived = forwardedMessage.DateReceived?.ToLocalDateTime();
            this.Sender = new ReadMessageViewModelProfile(forwardedMessage.Sender);
            this.Recipient = new ReadMessageViewModelProfile(forwardedMessage.Recipient);
            this.TemplateId = forwardedMessage.TemplateId;
            this.Subject = forwardedMessage.Subject;
            this.Rnu = forwardedMessage.Rnu;
            this.Body = forwardedMessage.Body;
            this.ForwardStatusId = forwardedMessage.ForwardStatusId;
            this.TemplateName = forwardedMessage.TemplateName;
            this.Blobs = forwardedMessage.Blobs
                .Select(e => new ReadMessageViewModelBlob(e))
                .ToList();
        }

        public int MessageId { get; set; }

        public DateTime DateSent { get; set; }

        public DateTime? DateReceived { get; set; }

        public ReadMessageViewModelProfile Sender { get; set; }

        public ReadMessageViewModelProfile Recipient { get; set; }

        public int TemplateId { get; set; }

        public string Subject { get; set; }

        public string Rnu { get; set; }

        public string Body { get; set; }

        public ED.DomainServices.ForwardStatus ForwardStatusId { get; set; }

        public string TemplateName { get; set; }

        public List<ReadMessageViewModelBlob> Blobs { get; set; }

        // component_guid -> (label, component_type, value)
        public Dictionary<Guid, FieldObject> Fields { get; set; } =
            new Dictionary<Guid, FieldObject>();

        public ReadMessageViewModel ForwardedMessage { get; set; }

        public void SetFields(
            string templateAsJson,
            string forwardedTemplateAsJson)
        {
            this.Fields = TemplatesService.GetFields(
               this.Body,
               templateAsJson);

            if (!string.IsNullOrEmpty(forwardedTemplateAsJson)
                && this.ForwardedMessage != null)
            {
                this.ForwardedMessage.Fields = TemplatesService.GetFields(
                    this.ForwardedMessage.Body,
                    forwardedTemplateAsJson);
            }
        }
    }
}
