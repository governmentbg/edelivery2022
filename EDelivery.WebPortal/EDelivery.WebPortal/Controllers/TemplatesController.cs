using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using ED.DomainServices.Templates;

using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models.Templates;
using EDelivery.WebPortal.Models.Templates.Blocks;
using EDelivery.WebPortal.Models.Templates.Components;

using static ED.DomainServices.Templates.Template;

namespace EDelivery.WebPortal.Controllers
{
    [AllowAnonymous]
    public partial class TemplatesController : BaseController
    {
        private readonly Lazy<TemplateClient> templateClient;

        public TemplatesController()
        {
            this.templateClient =
                new Lazy<TemplateClient>(
                    () => GrpcClientFactory.CreateTemplateClient(), isThreadSafe: false);
        }

        [HttpPost]
        public async Task<ActionResult> Builder(
            int id,
            BuilderContainer container)
        {
            GetContentResponse templateContent =
                await this.templateClient.Value.GetContentAsync(
                    new GetContentRequest
                    {
                        TemplateId = id
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            string json = templateContent.Content;

            List<BaseComponent> components =
                TemplatesService.ParseJsonWithValues(json, container.Values);

            if (container.Errors.Any())
            {
                foreach (BuilderModelStateError error in container.Errors)
                {
                    ModelState.AddModelError(error.Key.ToString(), error.Message);
                }
            }

            return PartialView("Partials/Builder", components);
        }

        [Route("test/dropdown")]
        public JsonResult GetDropdownOptions(string term)
        {
            List<SelectDataItem> options = new List<SelectDataItem>
            {
                new SelectDataItem { Id = "Понеделник", Text = "Понеделник", Selected = true, Disabled = false },
                new SelectDataItem { Id = "Вторник", Text = "Вторник", Selected = false, Disabled = false },
                new SelectDataItem { Id = "Сряда", Text = "Сряда", Selected = false, Disabled = false },
                new SelectDataItem { Id = "Четвъртък", Text = "Четвъртък", Selected = false, Disabled = false },
                new SelectDataItem { Id = "Петък", Text = "Петък", Selected = false, Disabled = false },
                new SelectDataItem { Id = "Събота", Text = "Събота", Selected = false, Disabled = true },
                new SelectDataItem { Id = "Неделя", Text = "Неделя", Selected = false, Disabled = true }
            };

            if (!string.IsNullOrEmpty(term))
            {
                options = options
                    .Where(e => e.Text.ToLowerInvariant().Contains(term.ToLowerInvariant()))
                    .ToList();
            }

            return Json(options, JsonRequestBehavior.AllowGet);
        }
    }
}
