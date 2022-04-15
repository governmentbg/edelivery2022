using System;

namespace EDelivery.WebPortal.Utils
{
    public class DateTimeHelper
    {
        public static bool EqualDate(DateTime source, DateTime target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            if (source.Year == target.Year
                && source.Month == target.Month
                && source.Day == target.Day)
            {
                return true;
            }

            return false;
        }
    }
}