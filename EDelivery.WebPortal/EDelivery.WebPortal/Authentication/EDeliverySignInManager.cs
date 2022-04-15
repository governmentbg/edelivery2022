using System.Security.Claims;
using System.Threading.Tasks;

using EDelivery.UserStore;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace EDelivery.WebPortal
{
    public class EDeliverySignInManager : SignInManager<Login, int>
    {
        public EDeliverySignInManager(
            EDeliveryUserManager userManager,
            IAuthenticationManager authenticationManager)
            : base(
                  userManager,
                  authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(Login user)
        {
            return user.GenerateUserIdentityAsync(
                (EDeliveryUserManager)UserManager);
        }

        public static EDeliverySignInManager Create(
            IdentityFactoryOptions<EDeliverySignInManager> options,
            IOwinContext context)
        {
            return new EDeliverySignInManager(
                context.GetUserManager<EDeliveryUserManager>(),
                context.Authentication);
        }
    }
}