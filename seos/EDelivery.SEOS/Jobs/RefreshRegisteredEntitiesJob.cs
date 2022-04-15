using log4net;
using System;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.Jobs
{
    public class RefreshRegisteredEntitiesJob : Job
    {
        public RefreshRegisteredEntitiesJob(string name) : base(name)
        {
        }

        protected override void Execute()
        {
            //refresh db entities

            try
            {
                var egovEntities = RegisteredEntitiesHelper.Load(logger);
                if (egovEntities == null)
                    return;

                DatabaseQueries.UpdateRegisteredEntities(egovEntities.Entities);
            }
            catch (Exception ex)
            {
                logger.Error("Can not update database registered entities", ex);
            }
        }
    }
}
