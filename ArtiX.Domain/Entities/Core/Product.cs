using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Inventory;

namespace ArtiX.Domain.Entities.Core;

public class Product : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
