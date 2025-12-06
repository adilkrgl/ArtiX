using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Inventory;
using ArtiX.Domain.Entities.Products;

namespace ArtiX.Domain.Entities.Core;

public class Product : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Guid? ProductTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public decimal CostPrice { get; set; } = 0m;

    public decimal RetailPrice { get; set; } = 0m;

    public decimal WholesalePrice { get; set; } = 0m;

    public Guid? ManufacturerId { get; set; }

    public Manufacturer? Manufacturer { get; set; }

    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;

    public Branch? Branch { get; set; }

    public ProductType? ProductType { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
