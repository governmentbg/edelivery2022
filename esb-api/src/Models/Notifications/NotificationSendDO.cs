using FluentValidation;

namespace ED.EsbApi;

// todo xml comment
public record NotificationSendDO(
    NotificationSendDOLegalEntity? LegalEntity,
    NotificationSendDOIndividual? Individual,
    NotificationSendDOEmail? Email,
    string? Sms,
    string? Viber);

public record NotificationSendDOLegalEntity(
    string Identifier,
    string Name,
    string? Email,
    string? Phone);

public record NotificationSendDOIndividual(
    string Identifier,
    string FirstName,
    string MiddleName,
    string LastName,
    string? Email,
    string? Phone);

public record NotificationSendDOEmail(
    string Subject,
    string Body);

public class NotificationSendDOLegalEntityValidator : AbstractValidator<NotificationSendDOLegalEntity>
{
    public NotificationSendDOLegalEntityValidator()
    {
        this.RuleFor(x => x.Identifier).NotNull().NotEmpty();
        this.RuleFor(x => x.Name).NotNull().NotEmpty();
        this.RuleFor(x => x.Email).EmailAddress().When(x => x is not null);
    }
}

public class NotificationSendDOIndividualValidator : AbstractValidator<NotificationSendDOIndividual>
{
    public NotificationSendDOIndividualValidator()
    {
        this.RuleFor(x => x.Identifier).NotNull().NotEmpty();
        this.RuleFor(x => x.FirstName).NotNull().NotEmpty();
        this.RuleFor(x => x.MiddleName).NotNull().NotEmpty();
        this.RuleFor(x => x.LastName).NotNull().NotEmpty();
        this.RuleFor(x => x.Email).EmailAddress().When(x => x is not null);
    }
}

public class NotificationSendDOEmailValidator : AbstractValidator<NotificationSendDOEmail>
{
    public NotificationSendDOEmailValidator()
    {
        this.RuleFor(x => x.Subject).NotNull().NotEmpty().MaximumLength(1000);
        this.RuleFor(x => x.Body).NotNull().NotEmpty();
    }
}

public class NotificationSendDOValidator : AbstractValidator<NotificationSendDO>
{
    public NotificationSendDOValidator()
    {
        this.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x =>
            {
                return x.LegalEntity is not null || x.Individual is not null;
            })
            .Must(x =>
            {
                return x.Email is not null || x.Sms is not null || x.Viber is not null;
            });

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        this.RuleFor(x => x.LegalEntity)
            .SetValidator(new NotificationSendDOLegalEntityValidator())
            .When(x => x.LegalEntity is not null);

        this.RuleFor(x => x.Individual)
            .SetValidator(new NotificationSendDOIndividualValidator())
            .When(x => x.Individual is not null);

        this.RuleFor(x => x.Email)
            .SetValidator(new NotificationSendDOEmailValidator())
            .When(x => x.Email is not null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        this.RuleFor(x => x.Sms).NotEmpty().MaximumLength(160).When(x => x.Sms is not null);
        this.RuleFor(x => x.Viber).NotEmpty().When(x => x.Viber is not null);
    }
}
