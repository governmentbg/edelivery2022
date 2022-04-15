using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models.Messages
{
    public class ViewMessageViewModel
    {
        public class ViewMessageViewModelProfile
        {
            public ViewMessageViewModelProfile(
                ED.DomainServices.Messages.ViewResponse.Types.MessageProfile messageProfile)
            {
                this.ProfileId = messageProfile.ProfileId;
                this.Name = messageProfile.Name;
            }

            public int ProfileId { get; set; }

            public string Name { get; set; }
        }

        public class ViewMessageViewModelBlob
        {
            public ViewMessageViewModelBlob(
                ED.DomainServices.Messages.ViewResponse.Types.MessageBlob blob)
            {
                this.BlobId = blob.BlobId;
                this.FileName = blob.FileName;
                this.Size = blob.Size;
                this.DocumentRegistrationNumber = blob.DocumentRegistrationNumber;
                this.Status = blob.Status;
                this.IsMalicious = blob.IsMalicious;

                this.Signatures =
                    blob.Signatures
                        .OrderBy(s => s.IsTimestamp)
                        .Select(s => new ViewMessageViewModelBlobSignature(s))
                        .ToList();
            }

            public int BlobId { get; set; }

            public string FileName { get; set; }

            public long? Size { get; set; }

            public string DocumentRegistrationNumber { get; set; }

            public ED.DomainServices.MalwareScanResultStatus Status { get; set; }

            public bool? IsMalicious { get; set; }

            public List<ViewMessageViewModelBlobSignature> Signatures { get; set; }
        }

        public class ViewMessageViewModelBlobSignature
        {
            public ViewMessageViewModelBlobSignature(
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

        public ViewMessageViewModel()
        {
        }

        public ViewMessageViewModel(
            ED.DomainServices.Messages.ViewResponse.Types.Message message)
        {
            this.MessageId = message.MessageId;
            this.DateSent = message.DateSent.ToLocalDateTime();
            this.Sender = new ViewMessageViewModelProfile(message.Sender);
            this.Recipients = message.Recipients;
            this.TemplateId = message.TemplateId;
            this.Subject = message.Subject;
            this.Orn = message.Orn;
            this.ReferencedOrn = message.ReferencedOrn;
            this.AdditionalIdentifier = message.AdditionalIdentifier;
            this.Body = message.Body;
            this.ForwardStatusId = message.ForwardStatusId;
            this.TemplateName = message.TemplateName;
            this.Blobs = message
                .Blobs
                .Select(e => new ViewMessageViewModelBlob(e))
                .ToList();
        }

        public ViewMessageViewModel(
            ED.DomainServices.Messages.ViewResponse.Types.Message message,
            ED.DomainServices.Messages.ViewResponse.Types.ForwardedMessage forwardedMessage)
            : this(message)
        {
            if (forwardedMessage != null)
            {
                this.ForwardedMessage = new ReadMessageViewModel(
                    forwardedMessage);
            }
        }

        public int MessageId { get; set; }

        public DateTime DateSent { get; set; }

        public ViewMessageViewModelProfile Sender { get; set; }

        public string Recipients { get; set; }

        public int TemplateId { get; set; }

        public string Subject { get; set; }

        public string Orn { get; set; }

        public string ReferencedOrn { get; set; }

        public string AdditionalIdentifier { get; set; }

        public string Body { get; set; }

        public ED.DomainServices.ForwardStatus ForwardStatusId { get; set; }

        public string TemplateName { get; set; }

        public List<ViewMessageViewModelBlob> Blobs { get; set; }

        // template_id -> (is_file, value)
        public Dictionary<string, (bool, object)> Fields { get; set; } =
            new Dictionary<string, (bool, object)>();

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
