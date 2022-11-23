namespace ED.EsbApi;

/// <summary>
/// Документ
/// </summary>
/// <param name="ProfileId">Публичен идентификатор на профил</param>
/// <param name="Name">Наименование на профил</param>
/// <param name="Identifier">Идентификатор на профил</param>
/// <param name="Email">Имейл на профил</param>
/// <param name="Phone">Телефон на профил</param>
public record ProfileDO(
    int ProfileId,
    string Identifier,
    string Name,
    string Email,
    string Phone);
