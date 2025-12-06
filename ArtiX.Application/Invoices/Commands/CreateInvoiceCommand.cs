using System;
using System.Collections.Generic;
using MediatR;

namespace ArtiX.Application.Invoices.Commands;

public class CreateInvoiceCommand : IRequest<Guid>
{
    public Guid CompanyId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CurrencyCode { get; set; } = "GBP";
    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
}
