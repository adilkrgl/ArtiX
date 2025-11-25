using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class QuotationLineConfiguration : IEntityTypeConfiguration<QuotationLine>
{
    public void Configure(EntityTypeBuilder<QuotationLine> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Quotation)
            .WithMany()
            .HasForeignKey(x => x.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);
    }
}
