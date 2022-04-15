using System.Collections.Generic;

namespace EDelivery.WebPortal.Models.Templates.Blocks
{
    public class SelectData
    {
        public List<SelectDataItem> Values { get; set; }
            = new List<SelectDataItem>();

        public string Url { get; set; }
    }
}