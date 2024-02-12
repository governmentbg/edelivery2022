using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

/// <summary>
/// Регистрация на профил на ЮЛ
/// </summary>
/// <param name="Identifier">Идентификатор на профила ЕИК/Булстат</param>
/// <param name="Name">Наименование на профил</param>
/// <param name="Email">Имейл за нотификация</param>
/// <param name="Phone">Телефон за нотификация</param>
/// <param name="Address">Адрес на профил</param>
/// <param name="TargetGroupId">Идентификатор на целева група, към която принадлежи профила</param>
/// <param name="OwnersData">Списък с данни на потребилите, които ще имат пълен достъп до регистрирания профил</param>
public record RegisterLegalEntityDO(
    string Identifier,
    string Name,
    string Email,
    string Phone,
    RegisterLegalEntityDOAddress Address,
    int TargetGroupId,
    RegisterLegalEntityDOOwnerData[] OwnersData);

/// <summary>
/// Данни за собственик на регистрирания профил
/// </summary>
/// <param name="Identifier">Идентификатор (ЕГН/ЛНЧ) на потребил</param>
/// <param name="Email">Имейл на потребител</param>
/// <param name="Phone">Телефон на потребител</param>
public record RegisterLegalEntityDOOwnerData(
    string Identifier,
    string Email,
    string Phone);

/// <summary>
/// Адрес на профил
/// </summary>
/// <param name="Residence">Адрес</param>
/// <param name="City">Наименование на град</param>
/// <param name="State">Наименование на община</param>
/// <param name="CountryIso">Код на страна</param>
public record RegisterLegalEntityDOAddress(
    string Residence,
    string? City,
    string? State,
    string? CountryIso);

public class RegisterLegalEntityDOValidator : AbstractValidator<RegisterLegalEntityDO>
{
    private class RegisterLegalEntityDOOwnerDataComparer : IEqualityComparer<RegisterLegalEntityDOOwnerData>
    {
        public bool Equals(RegisterLegalEntityDOOwnerData? x, RegisterLegalEntityDOOwnerData? y)
            => x?.Identifier == y?.Identifier;

        public int GetHashCode([DisallowNull] RegisterLegalEntityDOOwnerData obj)
            => obj.Identifier.GetHashCode();
    }

    private const int MaxOwners = 5;
    private readonly EsbClient esbClient;

    public RegisterLegalEntityDOValidator(EsbClient esbClient)
    {

        this.esbClient = esbClient;

        this.RuleFor(x => x.Identifier).NotNull();
        this.RuleFor(x => new { x.Identifier, x.TargetGroupId })
            .MustAsync(async (data, ct) =>
            {
                DomainServices.Esb.CheckExistingLegalEntityResponse resp =
                   await this.esbClient.CheckExistingLegalEntityAsync(
                       new DomainServices.Esb.CheckExistingLegalEntityRequest
                       {
                           Identifier = data.Identifier,
                           TargetGroupId = data.TargetGroupId,
                       },
                       cancellationToken: ct);

                return !resp.IsExisting;
            })
            .WithMessage("Can not duplicate legal entities");
        this.RuleFor(x => x.Name).NotNull();
        this.RuleFor(x => x.Email).EmailAddress();
        this.RuleFor(x => x.Phone).NotNull();
        this.RuleFor(x => x.Address)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(new RegisterLegalEntityDOAddressValidator(this.esbClient));
        this.RuleFor(x => x.TargetGroupId)
            .MustAsync(async (targetGroupId, ct) =>
            {
                DomainServices.Esb.CheckTargetGroupIdResponse resp =
                    await this.esbClient.CheckTargetGroupIdAsync(
                        new DomainServices.Esb.CheckTargetGroupIdRequest
                        {
                            TargetGroupId = targetGroupId,
                        },
                        cancellationToken: ct);

                return resp.IsValid;
            })
            .WithMessage("Unsupported target group");
        this.RuleFor(x => x.OwnersData)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(x => x.Count() <= MaxOwners)
            .WithMessage("{PropertyName} must contain fewer than {MaxOwners} entries")
            .Must(x => x.Distinct(new RegisterLegalEntityDOOwnerDataComparer()).Count() == x.Count())
            .WithMessage("{PropertyName} must contain unique identifiers")
            .MustAsync(async (ownersData, ct) =>
            {
                DomainServices.Esb.CheckAllLoginsExistResponse resp =
                    await this.esbClient.CheckAllLoginsExistAsync(
                        new DomainServices.Esb.CheckAllLoginsExistRequest
                        {
                            Identifiers =
                            {
                                ownersData.Select(e => e.Identifier),
                            }
                        },
                        cancellationToken: ct);

                return resp.IsValid;
            })
            .WithMessage("{PropertyName} contains user identificators that are not registered");
        this.RuleForEach(x => x.OwnersData)
            .SetValidator(new RegisterLegalEntityDOOwnerDataValidator());
    }
}

public class RegisterLegalEntityDOOwnerDataValidator : AbstractValidator<RegisterLegalEntityDOOwnerData>
{
    public RegisterLegalEntityDOOwnerDataValidator()
    {
        this.RuleFor(x => x.Identifier)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(identifier =>
            {
                return ProfileValidationUtils.IsValidEGN(identifier)
                    || ProfileValidationUtils.IsValidLNC(identifier);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.Email).EmailAddress();
        this.RuleFor(x => x.Phone).NotNull();
    }
}

public class RegisterLegalEntityDOAddressValidator : AbstractValidator<RegisterLegalEntityDOAddress>
{
    private readonly EsbClient esbClient;

    public RegisterLegalEntityDOAddressValidator(EsbClient esbClient)
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
