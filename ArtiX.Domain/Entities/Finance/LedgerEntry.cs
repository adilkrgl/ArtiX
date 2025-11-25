using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Finance;

public class LedgerEntry : BaseEntity
{
    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}
