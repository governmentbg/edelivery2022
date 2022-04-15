using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal abstract class EntityMapping : IEntityMapping
    {
        public abstract void AddFluentMapping(ModelBuilder builder);
    }
}
