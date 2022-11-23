using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ED.EsbApi;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[ApiVersionNeutral] // https://github.com/dotnet/aspnet-api-versioning/issues/174#issuecomment-321916143
public class ErrorController : ControllerBase
{
    [HttpGet("/api/error-local-development")]
    public IActionResult ErrorLocalDevelopment(
        [FromServices] IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.EnvironmentName != "Development")
        {
            throw new InvalidOperationException(
                "This shouldn't be invoked in non-development environments.");
        }

        IExceptionHandlerFeature? context =
            this.HttpContext.Features.Get<IExceptionHandlerFeature>();

        return this.Problem(
            detail: context?.Error.StackTrace ?? string.Empty,
            title: context?.Error.Message ?? "Unknown problem");
    }

    [HttpGet("/api/error")]
    public IActionResult Error() => this.Problem();
}
