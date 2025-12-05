using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Customers;

namespace ArtiX.Domain.Entities.Sales;

public class SalesOrder : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? SalesChannelId { get; set; }

    public SalesChannel? SalesChannel { get; set; }

    public Guid? SalesRepresentativeId { get; set; }

    public SalesRepresentative? SalesRepresentative { get; set; }

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; }

    public string? Status { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
