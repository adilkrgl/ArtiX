using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Sales;

public class DeliveryNote : BaseEntity
{
    public Guid CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? SalesChannelId { get; set; }

    public SalesChannel? SalesChannel { get; set; }

    public Guid? SalesRepresentativeId { get; set; }

    public SalesRepresentative? SalesRepresentative { get; set; }

    public ICollection<DeliveryNoteLine> Lines { get; set; } = new List<DeliveryNoteLine>();
}
