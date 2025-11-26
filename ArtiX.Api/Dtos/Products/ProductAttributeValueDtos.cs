namespace ArtiX.Api.Dtos.Products;

public class ProductAttributeValueDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public Guid AttributeValueId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string AttributeDisplayName { get; set; } = string.Empty;
    public string AttributeValue { get; set; } = string.Empty;
    public string? CustomValue { get; set; }
}

public class UpsertProductAttributeRequest
{
    public Guid AttributeDefinitionId { get; set; }
    public Guid? AttributeValueId { get; set; }
    public string? CustomValue { get; set; }
}
