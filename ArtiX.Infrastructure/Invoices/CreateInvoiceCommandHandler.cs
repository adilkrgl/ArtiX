using ArtiX.Application.Invoices;
using ArtiX.Application.Invoices.Commands;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Infrastructure.Invoices;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly ErpDbContext _dbContext;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;

    public CreateInvoiceCommandHandler(ErpDbContext dbContext, IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _dbContext = dbContext;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }

    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Lines
            .Where(l => l.ProductId.HasValue)
            .Select(l => l.ProductId!.Value)
            .Distinct()
            .ToList();

        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate,
            CurrencyCode = request.CurrencyCode,
            InvoiceNumber = await _invoiceNumberGenerator.GenerateNextAsync(request.InvoiceDate, cancellationToken),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineDto in request.Lines)
        {
            var product = lineDto.ProductId.HasValue
                ? products.FirstOrDefault(p => p.Id == lineDto.ProductId.Value)
                : null;

            var unitPrice = product?.RetailPrice ?? 0m;
            var taxRate = product?.TaxRate ?? 0m;
            var quantity = lineDto.Quantity;

            var lineSubtotal = quantity * unitPrice;
            var discountAmount = (lineDto.DiscountRate / 100m) * lineSubtotal;
            var lineTotal = lineSubtotal - discountAmount;
            var taxAmount = (taxRate / 100m) * lineTotal;
            var lineTotalWithTax = lineTotal + taxAmount;

            var line = new InvoiceLine
            {
                Id = Guid.NewGuid(),
                Invoice = invoice,
                InvoiceId = invoice.Id,
                ProductId = product?.Id,
                Product = product,
                ProductSku = product?.Sku,
                ProductName = product?.Name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountRate = lineDto.DiscountRate,
                DiscountAmount = discountAmount,
                LineSubtotal = lineSubtotal,
                LineTotal = lineTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                LineTotalWithTax = lineTotalWithTax,
                CustomDescription = lineDto.CustomDescription,
                LineNote = lineDto.LineNote,
                CreatedAt = DateTime.UtcNow
            };

            invoice.Lines.Add(line);
        }

        invoice.Subtotal = invoice.Lines.Sum(x => x.LineSubtotal);
        invoice.DiscountTotal = invoice.Lines.Sum(x => x.DiscountAmount);
        invoice.TaxTotal = invoice.Lines.Sum(x => x.TaxAmount);
        invoice.Total = invoice.Lines.Sum(x => x.LineTotalWithTax);

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
