using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.UserStore
{
    public partial class EDeliveryIdentityDB
    {
        public static EDeliveryIdentityDB Create()
        {
            return new EDeliveryIdentityDB();
        }
    }

    public partial class Login : IdentityUser<int, ExternalLogin,LoginsRole,LoginsClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Login,int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public partial class Role : IdentityRole<int, LoginsRole> { }

    public partial class LoginsRole : IdentityUserRole<int> { }

    public partial class ExternalLogin : IdentityUserLogin<int> { }

    public partial class LoginsClaim : IdentityUserClaim<int> { }
}
