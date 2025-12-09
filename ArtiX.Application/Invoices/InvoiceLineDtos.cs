using System;

namespace ArtiX.Application.Invoices;

public class CreateInvoiceLineDto
{
    public Guid? ProductId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountRate { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}

public class UpdateInvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountRate { get; set; }
    public string? CustomDescription { get; set; }
    public string? LineNote { get; set; }
}
