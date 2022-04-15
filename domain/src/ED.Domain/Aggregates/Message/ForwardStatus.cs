namespace ED.Domain
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    public enum ForwardStatus : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
    {
        None = 0,
        IsOriginalForwarded = 1,
        IsInForwardChain = 2,
        IsInForwardChainAndForwarded = 3
    }
}
