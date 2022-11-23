using System.Web;
using System.Web.Mvc;

using Markdig;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EDelivery.WebPortal.Extensions
{
    public static class HtmlHelperExtensions
    {
        // follow https://iterativo.wordpress.com/2013/04/04/converting-c-razor-models-into-javascript-objects/
        public static IHtmlString ToJson(this HtmlHelper helper, string obj)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var result = helper.Raw(
                JsonConvert.SerializeObject(
                    obj == null
                        ? null
                        : JsonConvert.DeserializeObject(obj), settings));

            return result;
        }

        public static IHtmlString MarkdownToHtml(this HtmlHelper helper, object value)
        {
            if (value is string str)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

                return helper.Raw(Markdig.Markdown.ToHtml(str, pipeline));
            }

            return new HtmlString(string.Empty);
        }
    }
}
