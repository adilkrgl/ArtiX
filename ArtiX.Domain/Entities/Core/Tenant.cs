using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }
}
