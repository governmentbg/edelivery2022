using ED.DomainServices.Translations;

using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models
{
    public class TranslationViewModel
    {
        public enum TranslationViewModelStatus
        {
            Pending = 1,
            Processing = 2,
            Processed = 3,
            Errored = 4,
        }

        public class TranslationRequestViewModel
        {
            public TranslationRequestViewModel() { }

            public TranslationRequestViewModel(GetTranslationResponse.Types.Request request)
            {
                this.RequestId = request.RequestId;
                this.SourceBlobId = request.SourceBlobId;
                this.SourceBlobFileName = request.SourceBlobFileName;
                this.TargetBlobId = request.TargetBlobId;
                this.TargetBlobFileName = request.TargetBlobFileName;
                this.Status = (TranslationViewModelStatus)request.Status;
                this.ErrorMessage = request.ErrorMessage;
            }

            public long? RequestId { get; set; }

            public int? SourceBlobId { get; set; }

            public string SourceBlobFileName { get; set; }

            public int? TargetBlobId { get; set; }

            public string TargetBlobFileName { get; set; }

            public TranslationViewModelStatus Status { get; set; }

            public string ErrorMessage { get; set; }

            public string StatusText
            {
                get
                {
                    if (!this.RequestId.HasValue || this.RequestId.Value > 0)
                    {
                        return string.Empty;
                    }

                    return EDeliveryResources.Translations.ResourceManager.GetString((Math.Abs(this.RequestId.Value)).ToString())
                         ?? EDeliveryResources.Translations._2;
                }
            }
        }

        public TranslationViewModel() { }

        public TranslationViewModel(
            GetTranslationResponse.Types.Translation translation)
        {
            this.MessageId = translation.MessageId;
            this.Subject = translation.Subject;
            this.SourceLanguage = translation.SourceLanguage;
            this.TargetLanguage = translation.TargetLanguage;
            this.CreateDate = translation.CreateDate.ToLocalDateTime();
            this.ModifyDate = translation.ModifyDate.ToLocalDateTime();

            this.Requests.AddRange(
                translation.Requests.Select(e => new TranslationRequestViewModel(e)));
        }

        public int MessageId { get; set; }

        public string Subject { get; set; }

        public string SourceLanguage { get; set; }

        public string TargetLanguage { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public List<TranslationRequestViewModel> Requests { get; set; } =
            new List<TranslationRequestViewModel>();
    }
}
