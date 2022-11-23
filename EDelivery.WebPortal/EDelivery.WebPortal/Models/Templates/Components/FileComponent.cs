namespace EDelivery.WebPortal.Models.Templates.Components
{
    public class FileComponent : BaseComponent
    {
        public FileComponent()
        {
            this.Type = ComponentType.file;
            this.FileIds = new int?[this.Instances];
            this.FileNames = new string[this.Instances];
            this.FileHashes = new string[this.Instances];
        }

        public void ForceInit()
        {
            this.FileIds = new int?[this.Instances];
            this.FileNames = new string[this.Instances];
            this.FileHashes = new string[this.Instances];
        }

        public int?[] FileIds { get; set; } // TODO: not really part of the component but rather form values

        public string[] FileNames { get; set; } // TODO: not really part of the component but rather form values

        public string[] FileHashes { get; set; } // TODO: not really part of the component but rather form values

        public int ExpirationPeriod { get; set; }

        public int MaxSize { get; set; }

        public string AllowedExtensions { get; set; }

        public int Instances { get; set; }
    }
}
