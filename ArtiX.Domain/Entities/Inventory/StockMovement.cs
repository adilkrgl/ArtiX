using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Inventory;

public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Reference { get; set; }
}
