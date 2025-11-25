using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Products;

public class AttributeValue : BaseEntity
{
    public Guid AttributeDefinitionId { get; set; }

    public AttributeDefinition AttributeDefinition { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
