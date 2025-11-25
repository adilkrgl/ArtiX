using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Crm;

public class Activity : BaseEntity
{
    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Subject { get; set; }
}
