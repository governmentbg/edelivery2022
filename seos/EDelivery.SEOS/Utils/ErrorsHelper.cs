using System;

namespace EDelivery.SEOS.Utils
{
    public class ErrorsHelper
    {
        public static string Describe(
            string text, 
            Exception ex)
        {
            if (ex == null)
                return text;

            if (ex.InnerException == null)
                return $"{text} {ex.Message}";

            if (ex.InnerException.InnerException == null)
                return $"{text} {ex.Message} {ex.InnerException.Message}";

            return $"{text} {ex.Message} {ex.InnerException.Message} " +
                $"{ex.InnerException.InnerException.Message}";
        }

        public static string Describe(
            Exception ex)
        {
            return Describe(String.Empty, ex);
        }

        public static string Describe(
            string text, 
            string additionalText)
        {
            if (String.IsNullOrEmpty(additionalText))
                return text;

            return $"{text} {additionalText}";
        }

        public static string Describe(
            string text, 
            Guid guid)
        {
            return Describe(text, guid.ToString("B"));
        }
    }
}
