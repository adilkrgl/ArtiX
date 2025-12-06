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
    public string CurrencyCode { get; set; } = "GBP";
    public decimal? ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
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
    public string CurrencyCode { get; set; } = "GBP";
    public List<CreateInvoiceLineRequest> Lines { get; set; } = new();
}

public class UpdateInvoiceRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesChannelId { get; set; }
    public Guid? SalesRepresentativeId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CurrencyCode { get; set; } = "GBP";
    public decimal? ExchangeRate { get; set; }
}
