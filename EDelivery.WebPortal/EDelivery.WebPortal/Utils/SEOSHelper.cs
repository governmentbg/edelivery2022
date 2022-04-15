using System.Collections.Generic;
using System.Web.Mvc;

using EDelivery.WebPortal.SeosService;

namespace EDelivery.WebPortal.Utils
{
    public static class SEOSHelper
    {
        /// <summary>
        /// Get status text
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSEOSStatusText(DocumentStatusType type)
        {
            switch (type)
            {
                case DocumentStatusType.DS_ALREADY_RECEIVED:
                    return EDeliveryResources.SEOS.StatusAlreadyReceived;
                case DocumentStatusType.DS_CLOSED:
                    return EDeliveryResources.SEOS.StatusClosed;
                case DocumentStatusType.DS_NOT_FOUND:
                    return EDeliveryResources.SEOS.StatusNotFound;
                case DocumentStatusType.DS_REGISTERED:
                    return EDeliveryResources.SEOS.StatusRegistered;
                case DocumentStatusType.DS_REJECTED:
                    return EDeliveryResources.SEOS.StatusRejected;
                case DocumentStatusType.DS_STOPPED:
                    return EDeliveryResources.SEOS.StatusStopped;
                case DocumentStatusType.DS_WAIT_REGISTRATION:
                    return EDeliveryResources.SEOS.StatusWaitRegistration;
                case DocumentStatusType.DS_SENT:
                    return EDeliveryResources.SEOS.StatusSent;
                case DocumentStatusType.DS_TRY_SEND:
                    return EDeliveryResources.SEOS.StatusSending;
                case DocumentStatusType.DS_SENT_FAILED:
                    return EDeliveryResources.SEOS.StatusSentFailed;
            }

            return string.Empty;
        }

        public static List<SelectListItem> SeosChangeStatusList()
        {
            var resultList = new List<SelectListItem>();
            var items = new List<DocumentStatusType>
            {
                DocumentStatusType.DS_WAIT_REGISTRATION,
                DocumentStatusType.DS_REGISTERED,
                DocumentStatusType.DS_STOPPED,
                DocumentStatusType.DS_REJECTED,
                DocumentStatusType.DS_CLOSED,
            };

            foreach (var x in items)
            {
                resultList.Add(
                    new SelectListItem()
                    {
                        Value = x.ToString(),
                        Text = GetSEOSStatusText((DocumentStatusType)x)
                    });
            }

            return resultList;
        }

        public static string GetEntityStatusText(EntityServiceStatusEnum status)
        {
            switch (status)
            {
                case EntityServiceStatusEnum.Active:
                    return EDeliveryResources.SEOS.EntityStatusActive;
                case EntityServiceStatusEnum.Inactive:
                    return EDeliveryResources.SEOS.EntityStatusInactive;
                case EntityServiceStatusEnum.TemporarilyInactive:
                    return EDeliveryResources.SEOS.EntityStatusTemporaryInactive;
            }

            return null;
        }
    }
}