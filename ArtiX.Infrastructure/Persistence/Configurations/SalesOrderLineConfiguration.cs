using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.SalesOrder)
            .WithMany()
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ProductId)
            .IsRequired(false);

        builder.Property(x => x.CustomDescription)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.LineNote)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);
    }
}
