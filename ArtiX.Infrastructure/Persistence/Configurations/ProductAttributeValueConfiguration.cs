using ArtiX.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomValue)
            .IsRequired(false);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AttributeValue)
            .WithMany()
            .HasForeignKey(x => x.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
