using ArtiX.Domain.Common;
using ArtiX.Domain.Enums;

namespace ArtiX.Domain.Entities.Core;

public class SalesChannel : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public SalesChannelType ChannelType { get; set; }

    public string? ExternalSystem { get; set; }

    public bool IsOnline { get; set; }

    public bool IsActive { get; set; }
}
