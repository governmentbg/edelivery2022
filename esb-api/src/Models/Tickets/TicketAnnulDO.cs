using System;
using FluentValidation;

namespace ED.EsbApi;

// todo xml comment
public record TicketAnnulDO(
    DateTime? AnnulDate,
    string AnnulmentReason);

public class TicketAnnulDOValidator : AbstractValidator<TicketAnnulDO>
{
    public TicketAnnulDOValidator()
    {
        this.RuleFor(x => x.AnnulDate).NotNull();
        this.RuleFor(x => x.AnnulmentReason).NotNull().NotEmpty();
    }
}
