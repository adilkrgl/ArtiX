using ArtiX.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.DataType)
            .IsRequired();

        builder.Property(x => x.IsVariant)
            .HasDefaultValue(false);

        builder.Property(x => x.IsFilterable)
            .HasDefaultValue(true);

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.HasMany(x => x.Values)
            .WithOne(v => v.AttributeDefinition)
            .HasForeignKey(v => v.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
