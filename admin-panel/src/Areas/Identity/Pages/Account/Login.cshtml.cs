// This files has been generetad with the identity generator
// and is intentionaly left as close as possible to the original

// dotnet aspnet-codegenerator identity -dc ED.AdminPanel.IdentityDbContext --files "Account.Login"

#pragma warning disable IDE0009 // Member access should be qualified.
#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ED.AdminPanel.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ED.AdminPanel.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly SignInManager<IdentityUser<int>> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            UserManager<IdentityUser<int>> userManager,
            SignInManager<IdentityUser<int>> signInManager,
            ILogger<LoginModel> logger)
        {

            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = nameof(LoginResources.UserName),
                ResourceType = typeof(LoginResources))]
            [Required(
                ErrorMessageResourceName = nameof(ErrorMessages.Required),
                ErrorMessageResourceType = typeof(ErrorMessages))]
            [DataType(DataType.Text)]
            public string UserName { get; set; }

            [Display(Name = nameof(LoginResources.Password),
                ResourceType = typeof(LoginResources))]
            [Required(
                ErrorMessageResourceName = nameof(ErrorMessages.Required),
                ErrorMessageResourceType = typeof(ErrorMessages))]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");

                    ModelState.AddModelError(string.Empty, LoginResources.InvalidLoginError);
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, LoginResources.InvalidLoginError);
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}