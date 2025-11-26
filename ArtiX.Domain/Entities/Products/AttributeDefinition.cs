using ArtiX.Domain.Common;
using ArtiX.Domain.Enums;

namespace ArtiX.Domain.Entities.Products;

public class AttributeDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public AttributeDataType DataType { get; set; } = AttributeDataType.Text;

    public bool IsVariant { get; set; } = false;

    public bool IsFilterable { get; set; } = true;

    public bool IsRequired { get; set; } = false;

    public int SortOrder { get; set; } = 0;

    public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
}
