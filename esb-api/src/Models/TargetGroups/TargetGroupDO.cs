namespace ED.EsbApi;

/// <summary>
/// Целева група
/// </summary>
/// <param name="TargetGroupId">Идентификатор на целева група</param>
/// <param name="Name">Наименование на целева група</param>
/// <param name="CanSelectRecipients">Възможност за преглед на профили от целевата група и възможност за избиране на получатели от нея</param>
public record TargetGroupDO(
    int TargetGroupId,
    string Name,
    bool CanSelectRecipients);
