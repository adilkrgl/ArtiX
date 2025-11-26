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

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? TaxNumber { get; set; }

    public bool IsActive { get; set; }
}
