using System;

namespace ED.AdminPanel
{
    public static class DateTimeExtensions
    {
        public static string ToQueryItem(this DateTime? dt)
        {
            return dt != null ? dt.Value.ToQueryItem() : string.Empty;
        }

        public static string ToQueryItem(this DateTime dt)
        {
            return dt.ToString(Constants.DateTimeFormat);
        }
    }
}
