using System;
using System.Collections.Generic;

namespace ED.Blobs
{
    public class RequirementContext
    {
        private Dictionary<string, object?> items;

        public RequirementContext()
        {
            this.items = new Dictionary<string, object?>();
        }

        public RequirementContext(IDictionary<string, object?> items)
        {
            this.items = new Dictionary<string, object?>(items);
        }

        public object? Get(string key)
        {
            if (!this.items.TryGetValue(key, out object? value))
            {
                return null;
            }

            if (value is not Func<object?>)
            {
                return value;
            }
            value = ((Func<object?>)value)();
            this.items[key] = value;

            return value;
        }

        public void Set(string key, object? value)
        {
            this.items.Add(key, value);
        }

        public void Set(string key, Func<object?> valueFactory)
        {
            this.items.Add(key, valueFactory);
        }
    }
}
