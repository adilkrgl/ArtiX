using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Customers;

public class CustomerContact : BaseEntity
{
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }
}
