using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Sales;

public class DeliveryNoteLine : BaseEntity
{
    public Guid DeliveryNoteId { get; set; }

    public DeliveryNote DeliveryNote { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
