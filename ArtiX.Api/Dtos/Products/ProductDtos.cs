using System;

namespace ArtiX.Api.Dtos.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductRequest
{
    public Guid? BranchId { get; set; }
    public Guid? ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; }
}
