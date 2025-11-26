namespace ArtiX.Api.Dtos.Core;

public class SalesChannelDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int ChannelType { get; set; }
    public string? ExternalSystem { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSalesChannelRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int ChannelType { get; set; }
    public string? ExternalSystem { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSalesChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int ChannelType { get; set; }
    public string? ExternalSystem { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
}
