using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Shared
{
    public partial class Pager
    {
        [Inject] private IStringLocalizer<Pager> Localizer { get; set; }

        private const int beginEndCount = 1;
        private const int prevNextCount = 1;
        private const int showFirstLastCount = 2;

        private record PageModel(
            int Number,
            bool IsCurrent,
            bool ShowDots
        );

        [Parameter] public int Page { get; set; } = 1;

        [Parameter] public int PageSize { get; set; } = 1;

        [Parameter] public int TotalItemsCount { get; set; } = 0;

        [Inject] NavigationManager NavigationManager { get; set; }

        private int totalPages;
        private int page;
        private PageModel[] pages;
        int from;
        int to;

        protected override void OnParametersSet()
        {
            this.totalPages =
                Math.Max(
                    (int)Math.Ceiling((double)this.TotalItemsCount / this.PageSize),
                    1);
            this.page = Math.Min(this.Page, this.totalPages);

            this.pages = this.CreatePager(this.page, this.totalPages);
            this.from = (this.page - 1) * this.PageSize + 1;
            this.to = Math.Min(this.page * this.PageSize, this.TotalItemsCount);
        }

        // Create a pages list like this
        // [1] [...] [5] [[6]] [7] [...] [100]
        private PageModel[] CreatePager(int curr, int length)
        {
            // calc break points
            int beginPoint = beginEndCount;
            int prevPoint = curr - prevNextCount;
            int nextPoint = curr + prevNextCount;
            int endPoint = length + 1 - beginEndCount;

            // always show the first/last N(showFirstLastCount)
            if (curr < showFirstLastCount - prevNextCount) {
              nextPoint = showFirstLastCount;
            }

            if (curr > length + 1 - showFirstLastCount + prevNextCount) {
              prevPoint = length + 1 - showFirstLastCount;
            }

            // coerce
            prevPoint = Math.Max(prevPoint, 1);
            nextPoint = Math.Min(nextPoint, length);
            beginPoint = Math.Min(beginPoint, prevPoint - 1);
            endPoint = Math.Max(endPoint, nextPoint + 1);

            IEnumerable<int> range(int first, int last)
                => Enumerable.Range(first, last - first + 1);

            IEnumerable<int> concat(params IEnumerable<int>[] enumerables)
                => enumerables.Aggregate(
                    Enumerable.Empty<int>(),
                    (seed, next) => seed.Concat(next));

            return concat(
                // the first few pages
                range(1, beginPoint),

                // skipped range or the single adjacent page to the first few
                beginPoint + 1 >= prevPoint ? Array.Empty<int>() :
                    beginPoint + 2 == prevPoint ? new[] { beginPoint + 1 } :
                    new[] { 0 },

                // pages adjacent on the left to the current
                range(prevPoint, curr - 1),

                // current page
                new[] { curr },

                // pages adjacent on the roght to the current
                range(curr + 1, nextPoint),

                // skipped range or the single adjacent page to the last few
                nextPoint + 1 >= endPoint ? Array.Empty<int>() :
                    nextPoint + 2 == endPoint ? new[] { nextPoint + 1 } :
                    new[] { 0 },

                // the last few pages
                range(endPoint, length))
                .Select(p => new PageModel(p, p == curr, p == 0))
                .ToArray();
        }

        private void NavigateToPage(int page)
        {
            this.NavigationManager.NavigateToSameWithQuery(
                new Dictionary<string, StringValues>
                {
                    { "page", page.ToString() }
                });
        }
    }
}
