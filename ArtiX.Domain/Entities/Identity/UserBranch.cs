using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Identity;

public class UserBranch : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
}
