using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Identity;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
