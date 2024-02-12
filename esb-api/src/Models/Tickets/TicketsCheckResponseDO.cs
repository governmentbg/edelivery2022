using System;

namespace ED.EsbApi;

// todo xml comment
public record TicketsCheckResponseDO(
    int TicketId,
    DateTime? DeliveryDate);
