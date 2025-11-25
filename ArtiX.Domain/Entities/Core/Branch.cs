using ArtiX.Domain.Common;
using ArtiX.Domain.Enums;

namespace ArtiX.Domain.Entities.Core;

public class Branch : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public BranchType BranchType { get; set; }
}
