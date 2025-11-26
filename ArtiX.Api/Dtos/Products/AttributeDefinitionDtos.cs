namespace ArtiX.Api.Dtos.Products;

public class AttributeDefinitionDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int DataType { get; set; }
    public bool IsVariant { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public class CreateAttributeDefinitionRequest
{
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int DataType { get; set; } = 1;
    public bool IsVariant { get; set; } = false;
    public bool IsFilterable { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}

public class UpdateAttributeDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int DataType { get; set; }
    public bool IsVariant { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}
