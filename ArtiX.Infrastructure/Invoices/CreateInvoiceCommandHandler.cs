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

            var unitPrice = lineDto.UnitPrice ?? product?.RetailPrice ?? 0m;
            var taxRate = product?.TaxRate ?? 0m;
            var isTaxInclusive = product?.IsTaxInclusive ?? false;
            var quantity = lineDto.Quantity;

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
                IsTaxInclusive = isTaxInclusive,
                DiscountRate = lineDto.DiscountRate,
                TaxRate = taxRate,
                CustomDescription = lineDto.CustomDescription,
                LineNote = lineDto.LineNote,
                CreatedAt = DateTime.UtcNow
            };

            RecalculateLine(line);
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

    private static void RecalculateLine(InvoiceLine line)
    {
        if (!line.IsTaxInclusive)
        {
            line.LineSubtotal = line.Quantity * line.UnitPrice;
            line.DiscountAmount = (line.DiscountRate / 100m) * line.LineSubtotal;
            line.LineTotal = line.LineSubtotal - line.DiscountAmount;
            line.TaxAmount = (line.TaxRate / 100m) * line.LineTotal;
            line.LineTotalWithTax = line.LineTotal + line.TaxAmount;
        }
        else
        {
            var grossTotal = line.Quantity * line.UnitPrice;
            line.DiscountAmount = (line.DiscountRate / 100m) * grossTotal;
            var grossAfterDiscount = grossTotal - line.DiscountAmount;

            if (line.TaxRate > 0)
            {
                var divisor = 1 + (line.TaxRate / 100m);
                var netAmount = grossAfterDiscount / divisor;

                line.LineSubtotal = netAmount;
                line.LineTotal = netAmount;

                line.TaxAmount = grossAfterDiscount - netAmount;
                line.LineTotalWithTax = grossAfterDiscount;
            }
            else
            {
                line.LineSubtotal = grossAfterDiscount;
                line.LineTotal = grossAfterDiscount;
                line.TaxAmount = 0m;
                line.LineTotalWithTax = grossAfterDiscount;
            }
        }
    }
}
