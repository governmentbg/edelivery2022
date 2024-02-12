namespace ED.EsbApi;

/// <summary>
/// Профил
/// </summary>
/// <param name="ProfileId">Публичен идентификатор на профил</param>
/// <param name="Name">Наименование на профил</param>
/// <param name="Identifier">Идентификатор на профил</param>
/// <param name="Email">Имейл на профил</param>
/// <param name="Phone">Телефон на профил</param>
/// <param name="Address">Адрес на профил</param>
public record ProfileDetailsDO(
    int ProfileId,
    string Identifier,
    string Name,
    string Email,
    string Phone,
    ProfileDetailsDOAddress? Address);

/// <summary>
/// Адрес на регистриран профил
/// </summary>
/// <param name="AddressId">Публичен идентификатор на адрес</param>
/// <param name="Residence">Адрес</param>
/// <param name="City">Наименование на град</param>
/// <param name="State">Наименование на община</param>
/// <param name="CountryIso">Код на страна</param>
public record ProfileDetailsDOAddress(
    int AddressId,
    string? Residence,
    string? City,
    string? State,
    string? CountryIso);
