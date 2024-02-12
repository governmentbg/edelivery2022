using System.ComponentModel;

namespace ED.Domain
{
    public enum TicketType
    {
        [Description("Електронен фиш")]
        Ticket = 1,
        [Description("Наказателно постановление")]
        PenalDecree = 2,
    }
}
