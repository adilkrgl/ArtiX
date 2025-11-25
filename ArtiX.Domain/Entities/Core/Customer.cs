using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Core;

public class Customer : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? DefaultSalesRepresentativeId { get; set; }

    public SalesRepresentative? DefaultSalesRepresentative { get; set; }
}
