using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Sales;

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public Guid? ProductId { get; set; }

    public Product? Product { get; set; }

    public string? ProductSku { get; set; }

    public string? ProductName { get; set; }

    public string? CustomDescription { get; set; }

    public string? LineNote { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountRate { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineSubtotal { get; set; }

    public decimal LineTotal { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal LineTotalWithTax { get; set; }
}
