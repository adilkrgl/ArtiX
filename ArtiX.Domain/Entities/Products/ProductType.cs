using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Products;

public class ProductType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
