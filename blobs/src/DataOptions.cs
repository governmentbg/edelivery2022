using System;

namespace ED.Blobs
{
    public class DataOptions
    {
#pragma warning disable CA1024 // Use properties where appropriate
        public string? GetConnectionString()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            if (this.ConnectionString?.Contains(
                    "Integrated Security=true",
                    StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new Exception("Win32 FILESTREAM access is only possible over Integrated Security connections.");
            }

            return this.ConnectionString;
        }

#pragma warning disable CA1721 // Property names should not match get methods
        public string? ConnectionString { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods
    }
}
