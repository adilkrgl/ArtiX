using System;

namespace ArtiX.Api.Dtos.Products;

public class ProductTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateProductTypeRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateProductTypeRequest
{
    public string Name { get; set; } = string.Empty;
}
