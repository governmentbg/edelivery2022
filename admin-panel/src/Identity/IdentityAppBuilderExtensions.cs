using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace ED.AdminPanel
{
    public static class IdentityAppBuilderExtensions
    {
        private static readonly HashSet<string> blockedIdentityPaths =
            new(
                new[]
                {
                    "/Identity/Account/ConfirmEmail",
                    "/Identity/Account/ConfirmEmailChange",
                    "/Identity/Account/ExternalLogin",
                    "/Identity/Account/ForgotPassword",
                    "/Identity/Account/ForgotPasswordConfirmation",
                    "/Identity/Account/Lockout",
                    "/Identity/Account/LoginWith2fa",
                    "/Identity/Account/LoginWithRecoveryCode",
                    "/Identity/Account/Manage",
                    "/Identity/Account/Manage/Index",
                    "/Identity/Account/Manage/ChangePassword",
                    "/Identity/Account/Manage/DeletePersonalData",
                    "/Identity/Account/Manage/Disable2fa",
                    "/Identity/Account/Manage/DownloadPersonalData",
                    "/Identity/Account/Manage/Email",
                    "/Identity/Account/Manage/EnableAuthenticator",
                    "/Identity/Account/Manage/ExternalLogins",
                    "/Identity/Account/Manage/GenerateRecoveryCodes",
                    "/Identity/Account/Manage/PersonalData",
                    "/Identity/Account/Manage/ResetAuthenticator",
                    "/Identity/Account/Manage/SetPassword",
                    "/Identity/Account/Manage/ShowRecoveryCodes",
                    "/Identity/Account/Manage/TwoFactorAuthentication",
                    "/Identity/Account/Register",
                    "/Identity/Account/RegisterConfirmation",
                    "/Identity/Account/ResendEmailConfirmation",
                    "/Identity/Account/ResetPassword",
                    "/Identity/Account/ResetPasswordConfirmation"
                },
                StringComparer.InvariantCultureIgnoreCase);

        public static IApplicationBuilder UseBlockedIdentityPages(this IApplicationBuilder app)
        {
            return app.Use((httpContext, next) =>
            {
                if (blockedIdentityPaths.Contains(httpContext.Request.Path.ToString().TrimEnd('/')))
                {
                    httpContext.Response.Redirect(httpContext.Request.PathBase);
                    return Task.CompletedTask;
                }

                return next();
            });
        }
    }
}
