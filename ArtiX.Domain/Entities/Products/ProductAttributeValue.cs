using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Products;

public class ProductAttributeValue : BaseEntity
{
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public Guid AttributeValueId { get; set; }

    public AttributeValue AttributeValue { get; set; } = null!;
}
