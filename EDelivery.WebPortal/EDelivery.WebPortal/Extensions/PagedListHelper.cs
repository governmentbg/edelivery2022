using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

using PagedList;
using PagedList.Mvc;

namespace EDelivery.WebPortal.Extensions
{
    public static class PagedListHelper
    {
        public static MvcHtmlString CustomPagedListPager(
            this System.Web.Mvc.HtmlHelper html,
            IPagedList list,
            Func<int, string> generatePageUrl,
            PagedListRenderOptions options)
        {
            if (options.Display == PagedListDisplayMode.Never
                || (options.Display == PagedListDisplayMode.IfNeeded
                && list.PageCount <= 1))
            {
                return null;
            }

            var listItemLinks = new List<TagBuilder>();

            //calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
            var lastPageToDisplay = list.PageCount;
            var pageNumbersToDisplay = lastPageToDisplay;
            if (options.MaximumPageNumbersToDisplay.HasValue && list.PageCount > options.MaximumPageNumbersToDisplay)
            {
                // cannot fit all pages into pager
                var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = list.PageNumber - maxPageNumbersToDisplay / 2;
                if (firstPageToDisplay < 1)
                    firstPageToDisplay = 1;
                pageNumbersToDisplay = maxPageNumbersToDisplay;
                lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;
                if (lastPageToDisplay > list.PageCount)
                    firstPageToDisplay = list.PageCount - maxPageNumbersToDisplay + 1;
            }

            //previous
            if (options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always
                || (options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded
                && !list.IsFirstPage))
            {
                listItemLinks.Add(Previous(list, generatePageUrl, options));
            }

            //page
            if (options.DisplayLinkToIndividualPages)
            {
                foreach (var i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
                {
                    //show page number link
                    listItemLinks.Add(Page(i, list, generatePageUrl, options));
                }
            }

            //next
            if (options.DisplayLinkToNextPage == PagedListDisplayMode.Always
                || (options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded
                && !list.IsLastPage))
            {
                listItemLinks.Add(Next(list, generatePageUrl, options));
            }

            if (listItemLinks.Any())
            {
                //append class to first item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                    listItemLinks.First().AddCssClass(options.ClassToApplyToFirstListItemInPager);

                //append class to last item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                    listItemLinks.Last().AddCssClass(options.ClassToApplyToLastListItemInPager);

                //append classes to all list item links
                foreach (var li in listItemLinks)
                    foreach (var c in options.LiElementClasses ?? Enumerable.Empty<string>())
                        li.AddCssClass(c);
            }

            //collapse all of the list items into one big string
            var listItemLinksString = listItemLinks.Aggregate(
                new StringBuilder(),
                (sb, listItem) => sb.Append(listItem.ToString()),
                sb => sb.ToString()
                );

            var div = new TagBuilder("div")
            {
                InnerHtml = listItemLinksString
            };
            div.AddCssClass("pagination-container");

            return new MvcHtmlString(div.ToString());
        }

        public static PagedListRenderOptions CustomEnableUnobtrusiveAjaxReplacing(
            AjaxOptions ajaxOptions)
        {
            PagedListRenderOptions options = new PagedListRenderOptions();

            options.FunctionToTransformEachPageLink = (_, aTagBuilder) =>
            {
                var aClass = aTagBuilder.Attributes.ContainsKey("class") ? aTagBuilder.Attributes["class"] ?? "" : "";

                if (ajaxOptions != null && !aClass.Contains("selected"))
                {
                    foreach (var ajaxOption in ajaxOptions.ToUnobtrusiveHtmlAttributes())
                        aTagBuilder.Attributes.Add(ajaxOption.Key, ajaxOption.Value.ToString());
                }

                return aTagBuilder;
            };
            return options;
        }

        private static TagBuilder Previous(
            IPagedList list,
            Func<int, string> generatePageUrl,
            PagedListRenderOptions options)
        {
            var targetPageNumber = list.PageNumber - 1;
            var previous = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToPreviousPageFormat, targetPageNumber)
            };

            previous.AddCssClass("arrow");

            previous.Attributes["rel"] = "prev";
            previous.Attributes["href"] = generatePageUrl(targetPageNumber);

            return Wrap(previous, options);
        }

        private static TagBuilder Page(
            int i,
            IPagedList list,
            Func<int, string> generatePageUrl,
            PagedListRenderOptions options)
        {
            var format = options.FunctionToDisplayEachPageNumber
                ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
            var targetPageNumber = i;
            var page = new TagBuilder("a");
            page.SetInnerText(format(targetPageNumber));

            if (i == list.PageNumber)
            {
                page.AddCssClass("selected");
            }
            else
            {
                page.Attributes["href"] = generatePageUrl(targetPageNumber);
            }

            return Wrap(page, options);
        }

        private static TagBuilder Next(
            IPagedList list,
            Func<int, string> generatePageUrl,
            PagedListRenderOptions options)
        {
            var targetPageNumber = list.PageNumber + 1;
            var next = new TagBuilder("a")
            {
                InnerHtml = string.Format(options.LinkToNextPageFormat, targetPageNumber)
            };

            next.AddCssClass("arrow");

            next.Attributes["rel"] = "next";
            next.Attributes["href"] = generatePageUrl(targetPageNumber);

            return Wrap(next, options);
        }

        private static TagBuilder Wrap(
            TagBuilder source,
            PagedListRenderOptions options)
        {
            if (options.FunctionToTransformEachPageLink != null)
            {
                return options.FunctionToTransformEachPageLink(null, source);
            }

            return source;
        }
    }
}