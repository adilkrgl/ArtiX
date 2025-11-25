using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Products;

namespace ArtiX.Domain.Entities.Inventory;

public class InventoryAttributeValue : BaseEntity
{
    public Guid InventoryItemId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;

    public Guid AttributeValueId { get; set; }

    public AttributeValue AttributeValue { get; set; } = null!;
}
