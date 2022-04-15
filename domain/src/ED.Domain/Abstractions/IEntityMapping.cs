using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal interface IEntityMapping
    {
        void AddFluentMapping(ModelBuilder builder);
    }
}
