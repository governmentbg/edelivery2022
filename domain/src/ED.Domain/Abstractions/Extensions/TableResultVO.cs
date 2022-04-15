using System;

namespace ED.Domain
{
    public record TableResultVO<T>(T[] Result, int Length);

    public static class TableResultVO
    {
        public static TableResultVO<T> Empty<T>()
        {
            return new TableResultVO<T>(Array.Empty<T>(), 0);
        }
    }
}
