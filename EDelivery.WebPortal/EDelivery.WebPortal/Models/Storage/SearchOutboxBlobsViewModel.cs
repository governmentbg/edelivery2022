using System;
using System.Globalization;

namespace EDelivery.WebPortal.Models
{
    public class SearchOutboxBlobsViewModel
    {
        public SearchOutboxBlobsViewModel()
        {
        }

        public string FileName { get; set; }

        public string MessageSubject { get; set; }

        public string FromDate { get; set; }

        public DateTime? ParsedFromDate
        {
            get
            {
                if (DateTime.TryParseExact(
                    this.FromDate,
                    SystemConstants.DatePickerDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fromDate))
                {
                    return fromDate;
                }

                return null;
            }
        }

        public string ToDate { get; set; }

        public DateTime? ParsedToDate
        {
            get
            {
                if (DateTime.TryParseExact(
                    this.ToDate,
                    SystemConstants.DatePickerDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime toDate))
                {
                    return toDate;
                }

                return null;
            }
        }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.FileName)
                    || !string.IsNullOrEmpty(this.MessageSubject)
                    || this.ParsedFromDate.HasValue
                    || this.ParsedToDate.HasValue;
            }
        }
    }
}