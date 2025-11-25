using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);
    }
}
