using System;
using FluentValidation;

namespace ED.EsbApi;

// todo xml comment
public record TicketServeDO(DateTime ServeDate);

public class TicketServeDOValidator : AbstractValidator<TicketServeDO>
{
    public TicketServeDOValidator()
    {
        this.RuleFor(x => x.ServeDate).NotNull();
    }
}
