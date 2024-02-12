namespace ED.EsbApi;

/// <summary>
/// Регистриран профил
/// </summary>
/// <param name="ProfileId">Публичен идентификатор на профил</param>
/// <param name="Name">Наименование на профил</param>
/// <param name="Identifier">Идентификатор на профил</param>
/// <param name="Email">Имейл на профил</param>
/// <param name="Phone">Телефон на профил</param>
/// <param name="Address">Адрес на профил</param>
/// <param name="IsActivated">Флаг, дали профилът е активиран</param>
/// <param name="IsPassive">Флаг, дали профилът е пасивен. ВАЖИ САМО ЗА ПРОФИЛИ НА ФИЗИЧЕСКИ ЛИЦА</param>
/// <param name="TargetGroupId">Идентификатор на целева група</param>
/// <param name="TargetGroupName">Наименование на целева група</param>
public record RegisteredProfileDO(
    int ProfileId,
    string Identifier,
    string Name,
    string Email,
    string Phone,
    RegisteredProfileDOAddress? Address,
    bool IsActivated,
    bool IsPassive,
    int TargetGroupId,
    string TargetGroupName);

/// <summary>
/// Адрес на регистриран профил
/// </summary>
/// <param name="AddressId">Публичен идентификатор на адрес</param>
/// <param name="Residence">Адрес</param>
/// <param name="City">Наименование на град</param>
/// <param name="State">Наименование на община</param>
/// <param name="CountryIso">Код на страна</param>
public record RegisteredProfileDOAddress(
    int AddressId,
    string? Residence,
    string? City,
    string? State,
    string? CountryIso);
