using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Products;

public class Manufacturer : BaseEntity
{
    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ProductNameAtManufacturer { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public string? ContactPerson { get; set; }

    public Company Company { get; set; } = null!;
}
