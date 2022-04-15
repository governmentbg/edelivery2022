using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.UserStore
{
   public class EDeliveryIdentitiesStore: Microsoft.AspNet.Identity.EntityFramework.UserStore<Login, Role, int, ExternalLogin, LoginsRole, LoginsClaim>
    {
        public EDeliveryIdentitiesStore(DbContext context):
            base(context)
        {
            context.Database.Log = (x) => { System.Diagnostics.Debug.Write(x); };
        }

        public Task<Login> FindByESubjectIdAsync(Guid eSubjectId)
        {            
            return base.GetUserAggregateAsync(u => u.ElectronicSubjectId == eSubjectId);
        }

        public override Task AddToRoleAsync(Login user, string roleName)
        {
            var role = Context.Set<Role>().FirstOrDefault(r => r.Name == roleName);
            if(role!=null)
            {
                Context.Set<LoginsRole>().Add(new LoginsRole(){ RoleId = role.Id, UserId=user.Id});
            }
            return Context.SaveChangesAsync();
        }
    }
}
