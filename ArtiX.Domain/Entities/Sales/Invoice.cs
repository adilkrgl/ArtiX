using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Customers;
using System.ComponentModel.DataAnnotations;

namespace ArtiX.Domain.Entities.Sales;

public class Invoice : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    [Required]
    [MaxLength(16)]
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

    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "GBP";

    public decimal? ExchangeRate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal Total { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
