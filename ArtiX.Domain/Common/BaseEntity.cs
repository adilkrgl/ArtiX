namespace ArtiX.Domain.Common;

public class BaseEntity
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
