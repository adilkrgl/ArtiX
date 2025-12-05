using System;
using System.Collections.Generic;

namespace ArtiX.Api.Dtos.Sales;

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
    public List<SalesOrderLineDto> Lines { get; set; } = new();
}

public class CreateSalesOrderRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime OrderDate { get; set; }
}

public class UpdateSalesOrderRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
}
