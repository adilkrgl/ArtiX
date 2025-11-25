using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public ICollection<SalesChannel> SalesChannels { get; set; } = new List<SalesChannel>();

    public ICollection<SalesRepresentative> SalesRepresentatives { get; set; } = new List<SalesRepresentative>();
}
