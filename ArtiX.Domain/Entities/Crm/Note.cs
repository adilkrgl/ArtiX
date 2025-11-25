using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Crm;

public class Note : BaseEntity
{
    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string Content { get; set; } = string.Empty;
}
