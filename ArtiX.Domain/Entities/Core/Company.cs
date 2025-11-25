using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }
}
