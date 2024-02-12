using System.Configuration;

namespace EDelivery.WebPortal
{
    public static class SystemConstants
    {
        static SystemConstants()
        {
            PageSize = 50;

            string pageSize = ConfigurationManager.AppSettings["pageSize"];

            if (!string.IsNullOrEmpty(pageSize))
            {
                if (int.TryParse(pageSize, out int parsedPageSize))
                {
                    PageSize =  parsedPageSize;
                }
            }
        }

        public const int ExportSize = 1000;

        public static int PageSize { get; set; }

        public const int LargePageSize = 100;

        public const int Select2PageSize = 50;

        public static readonly string DateTimeFormat = "dd.MM.yy HH:mm";

        public static readonly string DateFormat = "d MMM yyyy";

        public const string DatePickerDateFormat = "dd-MM-yyyy";

        public const string DateDefaultFormat = "yyyy-MM-dd";

        public const string PhoneRegex = @"^[0-9]{3}[0-9]{6,12}$";

        public const string ValidMobilePhone = @"^(3598|08)[6-9]\d{7}$";

        public const string CyrilicPattern = @"^[а-яА-Я\-\s]+$";

        public const int SystemTemplateId = 1;

        public const string OptionSeparator = "|";

        public const string EAuthCookieName = "eauthIdp";

        public const string EAuthCookieDomain = ".egov.bg";
    }
}
