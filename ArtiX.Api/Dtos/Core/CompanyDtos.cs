namespace ArtiX.Api.Dtos.Core;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public Guid? TenantId { get; set; }
}

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public Guid? TenantId { get; set; }
}

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public Guid? TenantId { get; set; }
}
