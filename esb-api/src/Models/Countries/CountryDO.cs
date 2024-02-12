namespace ED.EsbApi;

/// <summary>
/// Страна
/// </summary>
/// <param name="Iso">Идентификатор на страна (ISO-2)</param>
/// <param name="Name">Наименование на страна</param>
public record CountryDO(
    string Iso,
    string Name);
