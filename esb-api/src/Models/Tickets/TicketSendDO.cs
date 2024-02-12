using System;
using FluentValidation;

namespace ED.EsbApi;

// todo xml comment
public record TicketSendDO(
    TicketSendDOLegalEntity? LegalEntity,
    TicketSendDOIndividual Individual,
    TicketSendDOTicket Ticket);

public record TicketSendDOLegalEntity(
    string Identifier,
    string Name);

public record TicketSendDOIndividual(
    string Identifier,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Email,
    string? Phone);

public record TicketSendDOTicket(
    string Subject,
    string Body,
    TicketType Type,
    string? DocumentSeries,
    string DocumentNumber,
    DateTime IssueDate,
    string VehicleNumber,
    DateTime ViolationDate,
    string ViolatedProvision,
    string PenaltyProvision,
    string DueAmount,
    string DiscountedPaymentAmount,
    string IBAN,
    string BIC,
    string PaymentReason,
    TicketSendDOTicketBlob Document,
    string? DocumentIdentifier);

public record TicketSendDOTicketBlob(
    string Name,
    string ContentType,
    string Content,
    string? DocumentRegistrationNumber);

public enum TicketType
{
    Ticket = 1,
#pragma warning disable CA1707 // Identifiers should not contain underscores
    Penal_Decree = 2,
#pragma warning restore CA1707 // Identifiers should not contain underscores
}

// todo switch to manual validation
// https://docs.fluentvalidation.net/en/latest/aspnet.html

public class TicketSendDOLegalEntityValidator : AbstractValidator<TicketSendDOLegalEntity>
{
    public TicketSendDOLegalEntityValidator()
    {
        this.RuleFor(x => x.Identifier).NotNull().NotEmpty();
        this.RuleFor(x => x.Name).NotNull().NotEmpty();
    }
}

public class TicketSendDOIndividualValidator : AbstractValidator<TicketSendDOIndividual>
{
    public TicketSendDOIndividualValidator()
    {
        this.RuleFor(x => x.Identifier).NotNull().NotEmpty();
        this.RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);
        this.RuleFor(x => x.MiddleName)
            .MaximumLength(100)
            .When(x => x.MiddleName is not null);
        this.RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);
        this.RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);
    }
}

public class TicketSendDOTicketValidator : AbstractValidator<TicketSendDOTicket>
{
    public TicketSendDOTicketValidator()
    {
        this.RuleFor(x => x.Subject)
            .NotNull()
            .NotEmpty()
            .MaximumLength(255);
        this.RuleFor(x => x.Body).NotNull().NotEmpty();
        this.RuleFor(x => x.Type).IsInEnum();
        this.RuleFor(x => x.DocumentSeries)
            .NotNull()
            .NotEmpty()
            .When(x => x.Type == TicketType.Ticket);
        this.RuleFor(x => x.DocumentNumber).NotNull().NotEmpty();
        this.RuleFor(x => x.IssueDate).NotNull();
        this.RuleFor(x => x.VehicleNumber).NotNull().NotEmpty();
        this.RuleFor(x => x.ViolationDate).NotNull();
        this.RuleFor(x => x.ViolatedProvision).NotNull().NotEmpty();
        this.RuleFor(x => x.PenaltyProvision).NotNull().NotEmpty();
        this.RuleFor(x => x.DueAmount).NotNull().NotEmpty();
        this.RuleFor(x => x.DiscountedPaymentAmount).NotNull().NotEmpty();
        this.RuleFor(x => x.IBAN).NotNull().NotEmpty();
        this.RuleFor(x => x.BIC).NotNull().NotEmpty();
        this.RuleFor(x => x.PaymentReason).NotNull().NotEmpty();
        this.RuleFor(x => x.Document)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new TicketSendDOTicketBlobValidator());
        this.RuleFor(x => x.DocumentIdentifier)
            .MaximumLength(64)
            .When(x => x.DocumentIdentifier is not null);
    }
}

public class TicketSendDOTicketBlobValidator : AbstractValidator<TicketSendDOTicketBlob>
{
    public TicketSendDOTicketBlobValidator()
    {
        this.RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(500);
        this.RuleFor(x => x.ContentType).NotNull().NotEmpty();
        this.RuleFor(x => x.Content).NotNull().NotEmpty();
        this.RuleFor(x => x.DocumentRegistrationNumber)
                .NotEmpty()
                .MaximumLength(255)
                .When(x => x.DocumentRegistrationNumber is not null);
    }
}

public class TicketSendDOValidator : AbstractValidator<TicketSendDO>
{
    public TicketSendDOValidator()
    {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        this.RuleFor(x => x.LegalEntity)
            .SetValidator(new TicketSendDOLegalEntityValidator())
            .When(x => x.LegalEntity is not null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        this.RuleFor(x => x.Individual)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new TicketSendDOIndividualValidator());
        this.RuleFor(x => x.Ticket)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new TicketSendDOTicketValidator());
        ;
    }
}
