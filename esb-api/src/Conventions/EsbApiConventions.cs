using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

#pragma warning disable
#nullable disable

namespace ED.EsbApi;

public static class EsbApiConventions
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
    public static void Any(params object[] p)
    {
    }
}
