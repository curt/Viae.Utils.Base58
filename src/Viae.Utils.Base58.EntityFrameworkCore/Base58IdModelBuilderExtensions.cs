using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viae.Utils.Base58.Core;

namespace Viae.Utils.Base58.EntityFrameworkCore;

public static class Base58IdModelBuilderExtensions
{
    /// <summary>
    /// Configures all Base58Id properties in the model with appropriate
    /// converters, comparers, and column types.
    /// </summary>
    public static ModelBuilder ConfigureBase58Id(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Base58Id))
                {
                    property.SetValueConverter(new Base58IdValueConverter());
                    property.SetValueComparer(new Base58IdValueComparer());
                    property.SetColumnType("char(11)");

                    // Optionally set value generator for properties named "Id"
                    if (property.Name == "Id" || property.Name == entityType.ClrType.Name + "Id")
                    {
                        property.SetValueGeneratorFactory((p, e) => new Base58IdValueGenerator());
                    }
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures a specific Base58Id property with converter, comparer, and column type.
    /// </summary>
    public static PropertyBuilder<Base58Id> HasBase58IdConversion(
        this PropertyBuilder<Base58Id> propertyBuilder
    )
    {
        propertyBuilder.HasConversion(new Base58IdValueConverter()).HasColumnType("char(11)");

        propertyBuilder.Metadata.SetValueComparer(new Base58IdValueComparer());

        return propertyBuilder;
    }
}
