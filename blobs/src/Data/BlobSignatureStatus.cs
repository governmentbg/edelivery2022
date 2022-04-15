namespace ED.Blobs
{
    public enum BlobSignatureStatus
    {
        None = 0,
        Valid,
        InvalidIntegrity,
        CertificateExpiredAtTimeOfSigning,
        InvalidCertificate
    }
}
