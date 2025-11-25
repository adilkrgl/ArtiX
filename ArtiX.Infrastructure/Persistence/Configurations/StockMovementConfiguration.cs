using ArtiX.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany(w => w.StockMovements)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
