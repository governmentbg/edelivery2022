using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EDelivery.UserStore;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace EDelivery.WebPortal
{
    public class EDeliveryUserManager : UserManager<Login, int>
    {
        public EDeliveryUserManager(IUserStore<Login, int> store)
            : base(store)
        {
        }

        public Login FindByESubjectId(Guid eSubjectId)
        {
            return this.Users.SingleOrDefault(x => x.ElectronicSubjectId == eSubjectId);
        }

        public async Task<Login> FindByESubjectIdAsync(Guid eSubjectId)
        {
            return await this.Users.SingleOrDefaultAsync(x => x.ElectronicSubjectId == eSubjectId);
        }

        public string GenerateRandomPassword()
        {
            var validator = (PasswordValidator)this.PasswordValidator;

            string[] randomChars = new[]
            {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            for (int j = 0; j < 5; j++)
            {
                if (validator.RequireUppercase)
                    chars.Insert(rand.Next(0, chars.Count),
                        randomChars[0][rand.Next(0, randomChars[0].Length)]);

                if (validator.RequireLowercase)
                    chars.Insert(rand.Next(0, chars.Count),
                        randomChars[1][rand.Next(0, randomChars[1].Length)]);

                if (validator.RequireDigit)
                    chars.Insert(rand.Next(0, chars.Count),
                        randomChars[2][rand.Next(0, randomChars[2].Length)]);

                if (validator.RequireNonLetterOrDigit)
                    chars.Insert(rand.Next(0, chars.Count),
                        randomChars[3][rand.Next(0, randomChars[3].Length)]);

            }
            for (int i = chars.Count; i < validator.RequiredLength; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }
            return new string(chars.ToArray());
        }

        public static EDeliveryUserManager Create(
            IdentityFactoryOptions<EDeliveryUserManager> options,
            IOwinContext context)
        {
            var manager = new EDeliveryUserManager(
                new EDeliveryIdentitiesStore(
                    context.Get<EDeliveryIdentityDB>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<Login, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            IDataProtectionProvider dataProtectionProvider =
                options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<Login, int>(
                        dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }
}
