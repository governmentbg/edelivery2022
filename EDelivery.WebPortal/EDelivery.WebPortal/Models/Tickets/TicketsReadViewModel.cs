using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models.Tickets
{
    public class TicketsReadViewModel
    {
        public class TicketsReadViewModelTicket
        {
            public TicketsReadViewModelTicket(
                ED.DomainServices.Tickets.ReadResponse.Types.Ticket ticket,
                DateTime? internallyServedDate)
            {
                this.InternallyServedDate = internallyServedDate;
                this.Type = ticket.Type;
                this.Status = ticket.Status.Status;
                this.ServeDate = ticket.Status.ServeDate?.ToLocalDateTime();
                this.AnnulDate = ticket.Status.AnnulDate?.ToLocalDateTime();
                this.AnnulmentReason = ticket.Status.AnnulmentReason;
            }

            public DateTime? InternallyServedDate { get; set; }

            public string Type { get; set; }

            public ED.DomainServices.TicketStatusStatus Status { get; set; }

            public DateTime? ServeDate { get; set; }

            public DateTime? AnnulDate { get; set; }

            public string AnnulmentReason { get; set; }

            public string GetStatus
            {
                get
                {
                    switch(this.Status)
                    {
                        case ED.DomainServices.TicketStatusStatus.NonServed:
                            return "Невръчен административен акт"; // will never happen
                        case ED.DomainServices.TicketStatusStatus.InternallyServed:
                            return $"Връчен през ССЕВ на {this.InternallyServedDate:dd-MM-yyyy}";
                        case ED.DomainServices.TicketStatusStatus.ExternallyServed:
                            return $"Връчен извън ССЕВ на {this.ServeDate:dd-MM-yyyy}";
                        case ED.DomainServices.TicketStatusStatus.Annulled:
                            return $"Анулиран на {this.AnnulDate:dd-MM-yyyy} - {this.AnnulmentReason}";

                        default: 
                            return string.Empty;
                    }
                }
            }
        }

        public class TicketsReadViewModelProfile
        {
            public TicketsReadViewModelProfile(
                ED.DomainServices.Tickets.ReadResponse.Types.MessageProfile profile)
            {
                this.ProfileId = profile.ProfileId;
                this.Type = profile.Type;
                this.Name = profile.Name;
                this.IsReadOnly = profile.IsReadOnly;
                this.LoginName = profile.LoginName;
            }

            public int ProfileId { get; set; }

            public ED.DomainServices.ProfileType Type { get; set; }

            public string Name { get; set; }

            public bool IsReadOnly { get; set; }

            public string LoginName { get; set; }
        }

        public class TicketsReadViewModelBlob
        {
            public TicketsReadViewModelBlob(
                ED.DomainServices.Tickets.ReadResponse.Types.MessageBlob blob)
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
                        .Select(s => new TicketsReadViewModelBlobSignature(s))
                        .ToList();
            }

            public int BlobId { get; set; }

            public string FileName { get; set; }

            public string Hash { get; set; }

            public long? Size { get; set; }

            public string DocumentRegistrationNumber { get; set; }

            public ED.DomainServices.MalwareScanResultStatus Status { get; set; }

            public bool? IsMalicious { get; set; }

            public List<TicketsReadViewModelBlobSignature> Signatures { get; set; }
        }

        public class TicketsReadViewModelBlobSignature
        {
            public TicketsReadViewModelBlobSignature(
                ED.DomainServices.Tickets.ReadResponse.Types.MessageBlobSignature blobSignature)
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

        public TicketsReadViewModel(
            ED.DomainServices.Tickets.ReadResponse.Types.Message message)
        {
            this.MessageId = message.MessageId;
            this.DateSent = message.DateSent.ToLocalDateTime();
            this.DateReceived = message.DateReceived?.ToLocalDateTime();
            this.Ticket = new TicketsReadViewModelTicket(message.Ticket, this.DateReceived);
            this.Sender = new TicketsReadViewModelProfile(message.Sender);
            this.Recipient = new TicketsReadViewModelProfile(message.Recipient);
            this.Subject = message.Subject;
            this.Body = message.Body;
            this.Document = new TicketsReadViewModelBlob(message.Document);
            this.SafeBase64Url = message.SafeBase64Url;
        }

        public int MessageId { get; set; }

        public DateTime DateSent { get; set; }

        public DateTime? DateReceived { get; set; }

        public TicketsReadViewModelTicket Ticket { get; set; }

        public TicketsReadViewModelProfile Sender { get; set; }

        public TicketsReadViewModelProfile Recipient { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public TicketsReadViewModelBlob Document { get; set; }

        public string SafeBase64Url {  get; set; }

        public Dictionary<Guid, object> Fields
        {
            get
            {
                return JsonConvert.DeserializeObject<Dictionary<Guid, object>>(this.Body);
            }
        }
    }
}
