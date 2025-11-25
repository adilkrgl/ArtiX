using ArtiX.Domain.Common;
using ArtiX.Domain.Entities.Core;

namespace ArtiX.Domain.Entities.Crm;

public class TaskItem : BaseEntity
{
    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
