using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class SalesRepresentative : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }
}
