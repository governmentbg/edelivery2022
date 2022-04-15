using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

#nullable enable

namespace ED.AdminPanel
{
    public static class NavigationManagerExtensions
    {
        public static void NavigateToRelative(
            this NavigationManager navigationManager,
            string relativeUri)
        {
            string currentUri = navigationManager.Uri;

            if (!currentUri.EndsWith('/'))
            {
                currentUri = $"{currentUri}/";
            }

            string newUri = new Uri(
                new Uri(currentUri),
                new Uri(relativeUri, UriKind.Relative)).ToString();

            navigationManager.NavigateTo(newUri);
        }

        public static void NavigateToSameWithQuery(
            this NavigationManager navigationManager,
            IDictionary<string, StringValues> queryToMergeWith)
        {
            UriBuilder uriBuilder = new(navigationManager.Uri);

            Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uriBuilder.Query);
            foreach (var kvp in queryToMergeWith)
            {
                query.AddOrUpdate(kvp.Key, kvp.Value);
            }

            uriBuilder.Query = QueryHelpers.AddQueryString("", query);

            navigationManager.NavigateTo(uriBuilder.Uri.ToString());
        }

        public static T? GetCurrentQueryItem<T>(
            this NavigationManager navigationManager,
            string key)
        {
            Type type = typeof(T);
            Type underlyingType;
            bool isArray = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                underlyingType = Nullable.GetUnderlyingType(type)!;
            }
            else if (type.IsArray)
            {
                underlyingType = type.GetElementType()!;
                isArray = true;
            }
            else
            {
                underlyingType = type;
            }

            Uri uri = new(navigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out StringValues value))
            {
                if (isArray)
                {
                    Array arr = Array.CreateInstance(underlyingType, value.Count);
                    for (int i = 0; i < value.Count; i++)
                    {
                        arr.SetValue(Convert.ChangeType(value[i], underlyingType), i);
                    }
                    return (T)(object)arr;
                }
                else
                {
                    return (T)Convert.ChangeType(value.First(), underlyingType);
                }
            }
            else
            {
                return default(T);
            }
        }

        public static DateTime? GetQueryItemAsDateTime(
            this NavigationManager navigationManager,
            string key)
        {
            Uri uri = new(navigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out StringValues value))
            {
                return DateTime.ParseExact(
                    value,
                    Constants.DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None);
            }

            return null;
        }
    }
}
