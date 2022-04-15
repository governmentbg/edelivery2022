using System;

namespace ED.Blobs
{
    public record SignatureInfo (
        byte[] SigningCertificate,
        bool CoversDocument,
        bool CoversPriorRevision,
        bool IsTimestamp,
        DateTime SignDate,
        bool ValidAtTimeOfSigning,
        string Issuer,
        string Subject,
        string SerialNumber,
        int Version,
        DateTime ValidFrom,
        DateTime ValidTo);
}
