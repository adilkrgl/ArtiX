using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Sales;

public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
