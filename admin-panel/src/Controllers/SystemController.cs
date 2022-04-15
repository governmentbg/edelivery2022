using Microsoft.AspNetCore.Mvc;

namespace ED.AdminPanel.Controllers
{
    [Route("[controller]/[action]")]
    public class SystemController
    {
        public IActionResult Ping() => new OkResult();
    }
}
