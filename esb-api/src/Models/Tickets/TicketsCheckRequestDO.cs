using System;

namespace ED.EsbApi;

// todo xml comment
public record TicketsCheckRequestDO(
    int[] TicketIds,
    TicketsCheckRequestDODeliveryStatus? DeliveryStatus,
    DateTime? From,
    DateTime? To);

public enum TicketsCheckRequestDODeliveryStatus
{
    Delivered = 1,
    Undelivered = 2,
}
