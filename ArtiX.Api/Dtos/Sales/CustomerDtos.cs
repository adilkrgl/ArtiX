namespace ArtiX.Api.Dtos.Sales;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DefaultSalesRepresentativeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCustomerRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DefaultSalesRepresentativeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCustomerRequest
{
    public Guid? BranchId { get; set; }
    public Guid? DefaultSalesRepresentativeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerContactDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class CreateCustomerContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UpdateCustomerContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
