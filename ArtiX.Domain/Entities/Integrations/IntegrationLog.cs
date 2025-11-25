using ArtiX.Domain.Common;

namespace ArtiX.Domain.Entities.Integrations;

public class IntegrationLog : BaseEntity
{
    public string Source { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Payload { get; set; }
}
