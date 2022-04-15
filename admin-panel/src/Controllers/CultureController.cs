using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ED.AdminPanel.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController
    {
        [AllowAnonymous]
        public IActionResult Set(
            string culture,
            string redirectUri,
            [FromServices]IHttpContextAccessor httpContextAccessor)
        {
            if (culture != null)
            {
                httpContextAccessor.HttpContext!.AppendCultureCookie(culture, culture);
            }

            return new LocalRedirectResult(redirectUri);
        }
    }
}
