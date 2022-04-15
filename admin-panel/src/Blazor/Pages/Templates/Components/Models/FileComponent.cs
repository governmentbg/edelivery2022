using System.Text.Json.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields

namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public class FileComponent : BaseComponent
    {
        public FileComponent()
        {
            this.Type = ComponentType.file;
        }

        public string FileName { get; set; }

        [JsonInclude]
        [JsonPropertyName("FileId")]
        public string fileIdString;
        private int? fileId;
        [JsonIgnore]
        public int? FileId
        {
            get =>
                string.IsNullOrEmpty(this.fileIdString)
                    ? default(int?)
                    : this.fileId ??= int.Parse(this.fileIdString);
            set
            {
                this.fileId = value;
                this.fileIdString = value.ToString();
            }
        }

        [JsonInclude]
        [JsonPropertyName("ExpirationPeriod")]
        public string expirationPeriodString;
        private int? expirationPeriod;
        [JsonIgnore]
        public int ExpirationPeriod
        {
            get =>
                string.IsNullOrEmpty(this.expirationPeriodString)
                    ? default(int)
                    : this.expirationPeriod ??= int.Parse(this.expirationPeriodString);
            set
            {
                this.expirationPeriod = value;
                this.expirationPeriodString = value.ToString();
            }
        }

        [JsonInclude]
        [JsonPropertyName("MaxSize")]
        public string maxSizeString;
        private int? maxSize;
        [JsonIgnore]
        public int MaxSize
        {
            get =>
                string.IsNullOrEmpty(this.maxSizeString)
                    ? default(int)
                    : this.maxSize ??= int.Parse(this.maxSizeString);
            set
            {
                this.maxSize = value;
                this.maxSizeString = value.ToString();
            }
        }

        public string AllowedExtensions { get; set; }

        public int Instances { get; set; }
    }
}
