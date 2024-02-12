using System;

namespace ED.Domain
{
    public partial interface IEsbTicketsListQueryRepository
    {
        public record TicketsCheckVO(
            int TicketId,
            DateTime? DeliveryDate);
    }
}
