using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

using Newtonsoft.Json;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class HttpJsonModelBinder<T> : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(T))
                return false;

            try
            {
                var json = ExtractRequestJson(actionContext);
                bindingContext.Model = JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.Converters.StringEnumConverter());
                return true;
            }
            catch (JsonException exception)
            {
                bindingContext.ModelState.AddModelError("JsonDeserializationException", exception);
                return false;
            }
        }

        private string ExtractRequestJson(HttpActionContext actionContext)
        {
            var content = actionContext.Request.Content;
            string json = content.ReadAsStringAsync().Result;
            return json;
        }
    }
}