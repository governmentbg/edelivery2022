namespace ED.EsbApi;

/// <summary>
/// Списък обекти, използван при странициране
/// </summary>
/// <typeparam name="T">Тип обек</typeparam>
/// <param name="Result">Списък с обекти</param>
/// <param name="Length">Общ брой обекти</param>
public record TableResultDO<T>(T[] Result, int Length);
