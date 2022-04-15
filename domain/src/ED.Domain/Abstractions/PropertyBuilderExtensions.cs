using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ED.Domain
{
    static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<TProperty> ForSqlServerUseSpGetRangeSequenceHiLo<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder,
            string name,
            string schema,
            int blockSize)
        {
            propertyBuilder
                .ValueGeneratedOnAdd()
                .HasValueGenerator((property, entity) =>
                {
                    var model = property.DeclaringEntityType.Model;
                    var sequence = model.FindSequence(name, schema);

                    if (sequence == null)
                    {
                        throw new Exception($"Sequence \"{name}\" has not been added in the model, use modelBuilder.HasSequence(...).");
                    }

                    return new SqlServerSequenceHiLoValueGenerator<int>(sequence, blockSize);
                });

            return propertyBuilder;
        }
    }
}
