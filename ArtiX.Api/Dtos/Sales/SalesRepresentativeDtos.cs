namespace ArtiX.Api.Dtos.Sales;

public class SalesRepresentativeDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class CreateSalesRepresentativeRequest
{
    public Guid CompanyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UpdateSalesRepresentativeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
