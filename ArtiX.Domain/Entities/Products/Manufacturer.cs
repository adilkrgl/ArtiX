using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;
using ProductEntity = ArtiX.Domain.Entities.Core.Product;

namespace ArtiX.Domain.Entities.Products;

public class Manufacturer : BaseEntity
{
    public new Guid CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? ProductNameAtManufacturer { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public string? ContactPerson { get; set; }

    public Company Company { get; set; } = null!;

    public Branch? Branch { get; set; }

    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
}
