using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Products;

public class AttributeDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
