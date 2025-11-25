using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Integrations;

public class WebhookSubscription : BaseEntity
{
    public string Url { get; set; } = string.Empty;

    public string Event { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
