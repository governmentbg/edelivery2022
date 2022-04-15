using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal.Utils.DbAuditLogs
{
    public class EDeliveryLoggingDB : DbContext
    {
        public EDeliveryLoggingDB(string connectionString)
                : base(connectionString)
        {
            Database.SetInitializer<EDeliveryLoggingDB>(null);
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<JWTTokenAuditLog> JWTTokenAuditLogs { get; set; }
    }
}