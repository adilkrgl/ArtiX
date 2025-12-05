using System;
using System.Collections.Generic;

namespace ArtiX.Api.Dtos.Sales;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<InvoiceLineDto> Lines { get; set; } = new();
}

public class CreateInvoiceRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public List<CreateInvoiceLineItem> Lines { get; set; } = new();
}

public class UpdateInvoiceRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime InvoiceDate { get; set; }
}

public class CreateInvoiceLineItem
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}
