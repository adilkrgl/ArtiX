using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Sales;

public class QuotationLine : BaseEntity
{
    public Guid QuotationId { get; set; }

    public Quotation Quotation { get; set; } = null!;

    public Guid? ProductId { get; set; }

    public Product? Product { get; set; }

    public string? CustomDescription { get; set; }

    public string? LineNote { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
