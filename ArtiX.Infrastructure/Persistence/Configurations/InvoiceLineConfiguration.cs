using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ProductId)
            .IsRequired(false);

        builder.Property(x => x.ProductSku)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(x => x.ProductName)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.CustomDescription)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.LineNote)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 3);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountRate)
            .HasPrecision(5, 2);

        builder.Property(x => x.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsTaxInclusive)
            .HasDefaultValue(false);

        builder.Property(x => x.LineSubtotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.LineTotalWithTax)
            .HasPrecision(18, 2);
    }
}
