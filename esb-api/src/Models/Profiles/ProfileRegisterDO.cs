using FluentValidation;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

/// <summary>
/// Пасивна регистрация на профил на ЮЛ
/// </summary>
/// <param name="Identifier">Идентификатор на профила ЕГН/ЛНЧ</param>
/// <param name="FirstName">Собствено име</param>
/// <param name="MiddleName">Бащино име</param>
/// <param name="LastName">Фамилия</param>
/// <param name="Email">Имейл за нотификация</param>
/// <param name="Phone">Телефон за нотификация</param>
public record ProfileRegisterDO(
    string Identifier,
    string FirstName,
    string MiddleName,
    string LastName,
    string Email,
    string Phone,
    string Residence);

public class ProfileRegisterDOValidator : AbstractValidator<ProfileRegisterDO>
{
    private readonly EsbClient esbClient;

    public ProfileRegisterDOValidator(EsbClient esbClient)
    {
        this.esbClient = esbClient;

        this.RuleFor(x => x.Identifier).NotNull();
        this.RuleFor(x => x.Identifier)
            .Must(identifier =>
            {
                return ProfileValidationUtils.IsValidEGN(identifier)
                    || ProfileValidationUtils.IsValidLNC(identifier);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.Identifier)
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
        this.RuleFor(x => x.FirstName).NotNull();
        this.RuleFor(x => x.FirstName)
            .Must(firstName =>
            {
                return ProfileValidationUtils.IsValidName(firstName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.MiddleName).NotNull();
        this.RuleFor(x => x.MiddleName)
            .Must(middleName =>
            {
                return ProfileValidationUtils.IsValidName(middleName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.LastName).NotNull();
        this.RuleFor(x => x.LastName)
            .Must(lastName =>
            {
                return ProfileValidationUtils.IsValidName(lastName);
            })
            .WithMessage("{PropertyName} is not valid");
        this.RuleFor(x => x.Email).EmailAddress();
        this.RuleFor(x => x.Phone).NotNull();
        this.RuleFor(x => x.Residence).NotNull();
    }
}
