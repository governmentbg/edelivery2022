namespace EDelivery.WebPortal.Enums
{
    public enum eCertResponseStatus
    {
        InvalidResponseXML,
        InvalidSignature,
        AuthenticationFailed,
        Success,
        MissingEGN,
        CanceledByUser,
    }
}