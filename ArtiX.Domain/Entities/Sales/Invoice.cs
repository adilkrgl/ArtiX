using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Customers;

namespace ArtiX.Domain.Entities.Sales;

public class Invoice : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Guid? SalesChannelId { get; set; }

    public SalesChannel? SalesChannel { get; set; }

    public Guid? SalesRepresentativeId { get; set; }

    public SalesRepresentative? SalesRepresentative { get; set; }

    public DateTime InvoiceDate { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
