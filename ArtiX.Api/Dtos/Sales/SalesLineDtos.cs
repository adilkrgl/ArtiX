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
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineSubtotal { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotalWithTax { get; set; }
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
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateSalesOrderLineRequest
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class CreateInvoiceLineRequest
{
    public Guid? ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountRate { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateInvoiceLineRequest
{
    public Guid? ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountRate { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}
