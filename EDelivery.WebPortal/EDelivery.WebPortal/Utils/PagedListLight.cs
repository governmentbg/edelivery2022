using System.Collections.Generic;

namespace PagedList
{
    public class PagedListLight<T> : BasePagedList<T>
    {
        public PagedListLight(List<T> items, int pageSize, int pageNumber, int collectionCount) :
            base(pageNumber, pageSize, collectionCount)
        {
            base.Subset.AddRange(items);
        }
    }
}
