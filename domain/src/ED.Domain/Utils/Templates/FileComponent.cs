namespace ED.Domain
{
    public class FileComponent : BaseComponent
    {
        public FileComponent()
        {
            this.Type = ComponentType.file;
        }

        public int ExpirationPeriod { get; set; }

        public int MaxSize { get; set; }

        public string? AllowedExtensions { get; set; }

        public int Instances { get; set; }
    }
}
