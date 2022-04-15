using System;
using System.Collections.Generic;
using System.Linq;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.EGovRegstry;
using log4net;

namespace EDelivery.SEOS.Utils
{
    public class RegisteredEntitiesHelper
    {
        protected static readonly string docExchangeGuid = "{7060efa8-f1fa-4938-8b2b-12a78c86988f}";

        public static EGovMessageDir Load(ILog logger)
        {
            try
            {
                logger.Info("Load service entities started");
                using (var registryClient = new egovmsgPortTypeClient())
                {
                    var res = registryClient.GetAllRecords();
                    logger.Info("Service entities received:" + res?.Entities?.Count());
                    return res;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error loading registry entities", ex);
                return null;
            }
        }

        public static Service GetEntityService(Entity entity)
        {
            bool hasProperService = entity.Services.Any(
                y => y.Guid == docExchangeGuid &&
                !string.IsNullOrEmpty(y.URI) &&
                y.Status == eEntityServiceStatus.Active.ToString());

            return hasProperService
                ? entity.Services.First(
                    y => y.Guid == docExchangeGuid &&
                    !string.IsNullOrEmpty(y.URI) &&
                    y.Status == eEntityServiceStatus.Active.ToString())
                : entity.Services.FirstOrDefault(y => !string.IsNullOrEmpty(y.URI));
        }

        public static bool IsThroughEDelivery(RegisteredEntity entity)
        {
            if (entity == null)
                return false;

            var eDeliveryEntity = DatabaseQueries.GetEDeliveryEntity();

            if (entity.UniqueId == eDeliveryEntity.UniqueId)
                return true;

            if (!DatabaseQueries.HasProfile(entity.EIK))
                return false;

            if (entity.Status != 0 &&
                entity.CertificateSN == eDeliveryEntity.CertificateSN &&
                entity.ServiceUrl.ToLower() == eDeliveryEntity.ServiceUrl.ToLower())
                return true;

            return false;
        }

        public static bool IsThroughEDelivery(string uic)
        {
            var entity = DatabaseQueries.GetRegisteredEntity(uic);
            return IsThroughEDelivery(entity);
        }

        public static bool IsThroughEDelivery(Guid uniqueId)
        {
            var entity = DatabaseQueries.GetRegisteredEntity(uniqueId);
            return IsThroughEDelivery(entity);
        }
    }
}
