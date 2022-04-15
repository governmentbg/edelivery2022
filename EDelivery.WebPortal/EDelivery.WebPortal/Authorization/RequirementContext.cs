using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDelivery.WebPortal.Authorization
{
    public class RequirementContext
    {
        private Dictionary<string, object> items;

        public RequirementContext()
        {
            this.items = new Dictionary<string, object>();
        }

        public RequirementContext(IDictionary<string, object> items)
        {
            this.items = new Dictionary<string, object>(items);
        }

        public object Get(string key)
        {
            if (!this.items.TryGetValue(key, out object value))
            {
                return null;
            }

            if (value is Func<object>)
            {
                value = ((Func<object>)value)();
                this.items[key] = value;
            }

            return value;
        }

        public void Set(string key, object value)
        {
            this.toStringResult = null;
            this.items.Add(key, value);
        }

        public void Set(string key, Func<object> valueFactory)
        {
            this.toStringResult = null;
            this.items.Add(key, valueFactory);
        }

        private string toStringResult = null;
        public override string ToString()
        {
            if (this.toStringResult == null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string key in this.items.Keys.ToArray())
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(key);
                    sb.Append("=");

                    var value = this.Get(key);

                    string strValue = "";
                    if (value is IConvertible)
                    {
                        strValue = ((IConvertible)value).ToString(null);
                    }

                    sb.Append(strValue);
                }

                this.toStringResult = sb.ToString();
            }

            return this.toStringResult;
        }
    }
}