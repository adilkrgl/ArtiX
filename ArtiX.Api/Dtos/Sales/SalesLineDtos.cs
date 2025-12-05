using System;

namespace ArtiX.Api.Dtos.Sales;

public class QuotationLineDto
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class CreateQuotationLineRequest
{
    public Guid QuotationId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateQuotationLineRequest
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class CreateSalesOrderLineRequest
{
    public Guid SalesOrderId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateSalesOrderLineRequest
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class CreateInvoiceLineRequest
{
    public Guid InvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateInvoiceLineRequest
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}
