using ArtiX.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.HasOne(x => x.AttributeDefinition)
            .WithMany(d => d.Values)
            .HasForeignKey(x => x.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
