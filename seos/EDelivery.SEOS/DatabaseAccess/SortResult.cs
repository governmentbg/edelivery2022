using EDelivery.SEOS.DBEntities;
using System.Linq;
using System.Linq.Expressions;
using EDelivery.SEOS.DataContracts;

namespace EDelivery.SEOS.DatabaseAccess
{
    public class SortResult
    {
        public static IQueryable<SEOSMessage> Sort(IQueryable<SEOSMessage> result, SortColumnEnum sortColumn, bool descending)
        {
            string column = "DateCreated";

            switch (sortColumn)
            {
                case SortColumnEnum.DateReceived:
                case SortColumnEnum.DateSent:
                    column = "DateCreated";
                    break;
                case SortColumnEnum.DocKind:
                    column = "DocKind";
                    break;
                case SortColumnEnum.ReceiverName:
                    column = "Receiver.Name";
                    break;
                case SortColumnEnum.RegIndex:
                    column = "DocNumberInternal";
                    break;
                case SortColumnEnum.SenderName:
                    column = "Sender.Name";
                    break;
                case SortColumnEnum.Status:
                    column = "Status";
                    break;
                case SortColumnEnum.Title:
                    column = "DocAbout";
                    break;
                case SortColumnEnum.DocReferenceNumber:
                    column = "DocReferenceNumber";
                    break;
            }

            if (column == "Sender.Name")
            {
                return descending ? result.OrderByDescending(x => x.Sender.Name) : result.OrderBy(x => x.Sender.Name);
            }
            if (column == "Receiver.Name")
            {
                return descending ? result.OrderByDescending(x => x.Receiver.Name) : result.OrderBy(x => x.Receiver.Name);
            }

            var parameter = Expression.Parameter(typeof(SEOSMessage));
            var memberExpression = Expression.Property(Expression.Convert(parameter, typeof(SEOSMessage)), column);
            var expression = Expression.Lambda(memberExpression, parameter);

            return descending 
                ? Queryable.OrderByDescending(result, (dynamic)expression) 
                : Queryable.OrderBy(result, (dynamic)expression);
        }
    }
}
