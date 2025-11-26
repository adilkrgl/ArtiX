namespace ArtiX.Api.Dtos.Products;

public class AttributeValueDto
{
    public Guid Id { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CreateAttributeValueRequest
{
    public Guid AttributeDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}

public class UpdateAttributeValueRequest
{
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}
