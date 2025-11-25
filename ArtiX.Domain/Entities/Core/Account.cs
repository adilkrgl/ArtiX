using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class Account : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;
}
