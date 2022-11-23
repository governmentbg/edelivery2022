using System;
using System.Collections.Generic;
using System.Linq;

using EDelivery.WebPortal.Models.Templates.Components;

namespace EDelivery.WebPortal.Models
{
    public class ReadCodeMessageViewModel
    {
        public class ReadCodeMessageViewModelProfile
        {
            public ReadCodeMessageViewModelProfile(
                ED.DomainServices.CodeMessages.ReadResponse.Types.MessageProfile messageProfile)
            {
                this.ProfileId = messageProfile.ProfileId;
                this.ProfileName = messageProfile.ProfileName;
                this.LoginName = messageProfile.LoginName;
            }

            public int ProfileId { get; set; }

            public string ProfileName { get; set; }

            public string LoginName { get; set; }
        }

        public class ReadCodeMessageViewModelBlob
        {
            public ReadCodeMessageViewModelBlob(
                ED.DomainServices.CodeMessages.ReadResponse.Types.MessageBlob blob)
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
                        .Select(s => new ReadCodeMessageViewModelBlobSignature(s))
                        .ToList();
            }

            public int BlobId { get; set; }

            public string FileName { get; set; }

            public string Hash { get; set; }

            public long? Size { get; set; }

            public string DocumentRegistrationNumber { get; set; }

            public ED.DomainServices.MalwareScanResultStatus Status { get; set; }

            public bool? IsMalicious { get; set; }

            public List<ReadCodeMessageViewModelBlobSignature> Signatures { get; set; }
        }

        public class ReadCodeMessageViewModelBlobSignature
        {
            public ReadCodeMessageViewModelBlobSignature(
                ED.DomainServices.CodeMessages.ReadResponse.Types.MessageBlobSignature blobSignature)
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

        public ReadCodeMessageViewModel(
            ED.DomainServices.CodeMessages.ReadResponse.Types.Message message)
        {
            this.MessageId = message.MessageId;
            this.AccessCode = message.AccessCode.ToUpperInvariant();
            this.DateSent = message.DateSent.ToLocalDateTime();
            this.DateReceived = message.DateReceived?.ToLocalDateTime();
            this.Sender = new ReadCodeMessageViewModelProfile(message.Sender);
            this.Recipient = new ReadCodeMessageViewModelProfile(message.Recipient);
            this.TemplateId = message.TemplateId;
            this.Subject = message.Subject;
            this.Rnu = message.Rnu;
            this.Body = message.Body;
            this.TemplateName = message.TemplateName;
            this.Blobs = message.Blobs
                .Select(e => new ReadCodeMessageViewModelBlob(e))
                .ToList();
        }

        public int MessageId { get; set; }

        public string AccessCode { get; set; }

        public DateTime DateSent { get; set; }

        public DateTime? DateReceived { get; set; }

        public ReadCodeMessageViewModelProfile Sender { get; set; }

        public ReadCodeMessageViewModelProfile Recipient { get; set; }

        public int TemplateId { get; set; }

        public string Subject { get; set; }

        public string Rnu { get; set; }

        public string Body { get; set; }

        public string TemplateName { get; set; }

        public List<ReadCodeMessageViewModelBlob> Blobs { get; set; }

        // component_guid -> (label, component_type, value)
        public Dictionary<Guid, FieldObject> Fields { get; set; } =
            new Dictionary<Guid, FieldObject>();

        public void SetFields(Dictionary<Guid, FieldObject> values)
        {
            foreach (KeyValuePair<Guid, FieldObject> item in values)
            {
                if (this.Fields.ContainsKey(item.Key))
                {
                    this.Fields[item.Key] = item.Value;
                }
                else
                {
                    this.Fields.Add(item.Key, item.Value);
                }
            }
        }
    }
}
