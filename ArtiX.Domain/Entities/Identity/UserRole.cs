using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Identity;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;
}
