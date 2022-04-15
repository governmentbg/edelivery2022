namespace ED.Blobs
{
    public static class BlobConstants
    {
        // from System.IO.Stream source
        // We pick a value that is the largest multiple of 4096
        // that is still smaller than the large object heap threshold (85K). 
        // The CopyTo/CopyToAsync buffer is short-lived and is likely
        // to be collected at Gen0, and it offers a significant 
        // improvement in Copy performance. 
        public const int DefaultCopyBufferSize = 81920;

        // The pipe does not provide options to control Min/Max segments
        // and the defaults are not geared for good performance of copying large files
        // see https://github.com/dotnet/runtime/issues/43480
        public const int PipeMinimumSegmentSize = 16 * 1024; // 16K
        public const int PipePauseWriterThreshold = 1014 * 1024; // 1MB
        public const int PipeResumeWriterThreshold = PipePauseWriterThreshold / 2;

        public const int AnonymousLoginId = 1;
        public const int SystemProfileId = 1;

        public const long DefaultStorageQuota = 1 * 1024 * 1024 * 1024; // 1GB
    }
}
