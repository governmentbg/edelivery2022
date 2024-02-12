using System;
using System.Text;

namespace ED.Domain
{
    public static class StringUtils
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        public static string ToPhone(this string phone)
        {
            if (phone.StartsWith("+"))
            {
                return phone[1..];
            }
            else if (phone.StartsWith("08"))
            {
                return $"359{phone[1..]}";
            }
            else if (phone.StartsWith("09"))
            {
                return $"359{phone[1..]}";
            }

            return phone;
        }

        public static string ToUrlSafeBase64(this string message)
        {
            return Convert
                .ToBase64String(Encoding.UTF8.GetBytes(message))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
