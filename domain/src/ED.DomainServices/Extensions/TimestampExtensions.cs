using System;

namespace ED.DomainServices
{
    public static class TimestampExtensions
    {
        public static Google.Protobuf.WellKnownTypes.Timestamp ToTimestamp(
            this DateTime dateTime)
        {
            return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime());
        }

        public static DateTime ToLocalDateTime(
            this Google.Protobuf.WellKnownTypes.Timestamp timestamp)
        {
            return timestamp.ToDateTime().ToLocalTime();
        }
    }
}
