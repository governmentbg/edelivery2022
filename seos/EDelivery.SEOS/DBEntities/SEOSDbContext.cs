using System.Data.Entity;

namespace EDelivery.SEOS.DBEntities
{
    public class SEOSDbContext : DbContext
    {
        public SEOSDbContext(string connectionString)
                : base(connectionString)
        {
            Database.SetInitializer<SEOSDbContext>(null);
            this.Configuration.LazyLoadingEnabled = false;

#if DEBUG
            this.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
#endif
        }

        public virtual DbSet<SEOSCommunicationLog> SEOSCommunicationLogs { get; set; }

        public virtual DbSet<RegisteredEntity> RegisteredEntities { get; set; }
        public virtual DbSet<SEOSMessage> SEOSMessages { get; set; }
        public virtual DbSet<SEOSMessageAttachment> SEOSMessageAttachments { get; set; }
        public virtual DbSet<SEOSMessageCorespondent> SEOSMessageCorespondents { get; set; }
        public virtual DbSet<SEOSStatusHistory> SEOSStatusHistories { get; set; }
        public virtual DbSet<SEOSRetrySendQueueItem> SEOSRetrySendQueue { get; set; }
        public virtual DbSet<MalwareScanResult> MalwareScanResults { get; set; }
        public virtual DbSet<AS4RegisteredEntity> AS4RegisteredEntities { get; set; }
        public virtual DbSet<AS4MessagesTransferLog> AS4MessagesTransferLog { get; set; }
        public virtual DbSet<AS4SentMessagesStatus> AS4SentMessagesStatus { get; set; }
    }
}
