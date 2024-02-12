using FluentValidation;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

/// <summary>
/// Пасивна регистрация на профил на ФЛ
/// </summary>
/// <param name="Identifier">Идентификатор на профила ЕГН/ЛНЧ</param>
/// <param name="FirstName">Собствено име</param>
/// <param name="MiddleName">Бащино име</param>
/// <param name="LastName">Фамилия</param>
/// <param name="Email">Имейл за нотификация</param>
/// <param name="Phone">Телефон за нотификация</param>
/// <param name="Address">Адрес на профил</param>
public record RegisterPassiveIndividualDO(
    string Identifier,
    string FirstName,
    string MiddleName,
    string LastName,
    string Email,
    string Phone,
    RegisterPassiveIndividualDOAddress Address);

/// <summary>
/// Адрес на профил
/// </summary>
/// <param name="Residence">Адрес</param>
/// <param name="City">Наименование на град</param>
/// <param name="State">Наименование на община</param>
/// <param name="CountryIso">Код на страна</param>
public record RegisterPassiveIndividualDOAddress(
    string Residence,
    string? City,
    string? State,
    string? CountryIso);

public class RegisterPassiveIndividualDOValidator : AbstractValidator<RegisterPassiveIndividualDO>
{
    private readonly EsbClient esbClient;

    public RegisterPassiveIndividualDOValidator(EsbClient esbClient)
    {
        this.esbClient = esbClient;

        this.RuleFor(x => x.Identifier)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(identifier =>
            {
                return ProfileValidationUtils.IsValidEGN(identifier)
                    || ProfileValidationUtils.IsValidLNC(identifier);
            })
            .WithMessage("{PropertyName} is not valid")
            .MustAsync(async (identifier, ct) =>
            {
                DomainServices.Esb.CheckExistingIndividualResponse resp =
                   await this.esbClient.CheckExistingIndividualAsync(
                       new DomainServices.Esb.CheckExistingIndividualRequest
                       {
                           Identifier = identifier,
                       },
                       cancellationToken: ct);

                return !resp.IsExisting;
            })
            .WithMessage("Can not duplicate individuals");
        this.RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(firstName =>
            {
                return ProfileValidationUtils.IsValidName(firstName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.MiddleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(middleName =>
            {
                return ProfileValidationUtils.IsValidName(middleName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(lastName =>
            {
                return ProfileValidationUtils.IsValidName(lastName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.Email).EmailAddress();
        this.RuleFor(x => x.Phone).NotNull();
        this.RuleFor(x => x.Address)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new RegisterPassiveIndividualDOAddressValidator(this.esbClient));
    }
}

public class RegisterPassiveIndividualDOAddressValidator : AbstractValidator<RegisterPassiveIndividualDOAddress>
{
    private readonly EsbClient esbClient;

    public RegisterPassiveIndividualDOAddressValidator(EsbClient esbClient)
    {
        this.esbClient = esbClient;

        this.RuleFor(x => x.Residence).NotNull();
        this.RuleFor(x => x.CountryIso)
            .MustAsync(async (iso, ct) =>
            {
                DomainServices.Esb.CheckCountryIsoResponse resp =
                    await this.esbClient.CheckCountryIsoAsync(
                        new DomainServices.Esb.CheckCountryIsoRequest
                        {
                            Iso = iso,
                        },
                        cancellationToken: ct);

                return resp.IsValid;
            })
            .When(x => !string.IsNullOrEmpty(x.CountryIso))
            .WithMessage("Unsupported country iso");
    }
}
