using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcPartialList<T>
    {
        public DcPartialList()
        {

        }

        public DcPartialList(int itemsCount, List<T> items)
        {
            this.AllItemsCount = itemsCount;
            this.Items = items;
        }

        public DcPartialList(int itemsCount, List<T> items, Enums.eSortColumn sortColumn, Enums.eSortOrder sortOrder)
        {
            this.AllItemsCount = itemsCount;
            this.Items = items;
            this.SortColumn = sortColumn;
            this.SortOrder = sortOrder;
        }

        [DataMember]
        public int AllItemsCount { get; set; }

        [DataMember]
        public List<T> Items { get; set; }

        [DataMember]
        public Enums.eSortColumn SortColumn { get; set; }

        [DataMember]
        public Enums.eSortOrder SortOrder { get; set; }
    }
}
