using ArtiX.Domain.Common;
namespace ArtiX.Domain.Entities.Core;

public class Warehouse : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }
}
