using ArtiX.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(ii => ii.Id);

        builder.HasOne(ii => ii.Warehouse)
            .WithMany()
            .HasForeignKey(ii => ii.WarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ii => ii.Product)
            .WithMany()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
