using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InvoiceLinesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public InvoiceLinesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceLineDto>>> GetAsync([FromQuery] Guid? invoiceId)
    {
        var query = _db.InvoiceLines.AsQueryable();

        if (invoiceId.HasValue)
        {
            query = query.Where(x => x.InvoiceId == invoiceId.Value);
        }

        var items = await query
            .Select(x => new InvoiceLineDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductSku = x.ProductSku,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                IsTaxInclusive = x.IsTaxInclusive,
                DiscountRate = x.DiscountRate,
                DiscountAmount = x.DiscountAmount,
                LineSubtotal = x.LineSubtotal,
                LineTotal = x.LineTotal,
                TaxRate = x.TaxRate,
                TaxAmount = x.TaxAmount,
                LineTotalWithTax = x.LineTotalWithTax,
                CustomDescription = x.CustomDescription,
                LineNote = x.LineNote
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceLineDto>> GetByIdAsync(Guid id)
    {
        var dto = await _db.InvoiceLines
            .Where(x => x.Id == id)
            .Select(x => new InvoiceLineDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductSku = x.ProductSku,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                IsTaxInclusive = x.IsTaxInclusive,
                DiscountRate = x.DiscountRate,
                DiscountAmount = x.DiscountAmount,
                LineSubtotal = x.LineSubtotal,
                LineTotal = x.LineTotal,
                TaxRate = x.TaxRate,
                TaxAmount = x.TaxAmount,
                LineTotalWithTax = x.LineTotalWithTax,
                CustomDescription = x.CustomDescription,
                LineNote = x.LineNote
            })
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            return NotFound();
        }

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceLineDto>> CreateAsync([FromQuery] Guid invoiceId, [FromBody] CreateInvoiceLineRequest request)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            return BadRequest(new { message = "Invoice not found." });
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, invoiceId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        Product? product = null;
        if (request.ProductId.HasValue)
        {
            product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId.Value);
        }

        var unitPrice = request.UnitPrice ?? product?.RetailPrice ?? 0m;
        var taxRate = product?.TaxRate ?? 0m;
        var isTaxInclusive = product?.IsTaxInclusive ?? false;

        var line = new InvoiceLine
        {
            Id = Guid.NewGuid(),
            Invoice = invoice,
            InvoiceId = invoice.Id,
            ProductId = product?.Id,
            Product = product,
            ProductSku = product?.Sku,
            ProductName = product?.Name,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            IsTaxInclusive = isTaxInclusive,
            DiscountRate = request.DiscountRate,
            TaxRate = taxRate,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CreatedAt = DateTime.UtcNow
        };

        RecalculateLine(line);

        _db.InvoiceLines.Add(line);
        invoice.Lines.Add(line);

        RecalculateInvoiceTotals(invoice);

        await _db.SaveChangesAsync();

        return Ok(ToDto(line));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceLineDto>> UpdateAsync(Guid id, [FromBody] UpdateInvoiceLineRequest request)
    {
        var line = await _db.InvoiceLines
            .Include(l => l.Invoice)
            .ThenInclude(i => i.Lines)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (line == null)
        {
            return NotFound();
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, line.InvoiceId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        Product? product = null;
        if (request.ProductId.HasValue)
        {
            product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId.Value);
        }

        line.ProductId = product?.Id;
        line.Product = product;
        line.ProductSku = product?.Sku;
        line.ProductName = product?.Name;
        line.Quantity = request.Quantity;
        if (request.UnitPrice.HasValue)
        {
            line.UnitPrice = request.UnitPrice.Value;
        }
        else if (product != null)
        {
            line.UnitPrice = product.RetailPrice;
        }
        line.DiscountRate = request.DiscountRate;
        line.IsTaxInclusive = product?.IsTaxInclusive ?? line.IsTaxInclusive;
        line.TaxRate = product?.TaxRate ?? line.TaxRate;
        line.CustomDescription = request.CustomDescription;
        line.LineNote = request.LineNote;
        line.UpdatedAt = DateTime.UtcNow;

        RecalculateLine(line);

        if (line.Invoice != null)
        {
            RecalculateInvoiceTotals(line.Invoice);
        }

        await _db.SaveChangesAsync();

        return Ok(ToDto(line));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var line = await _db.InvoiceLines
            .Include(l => l.Invoice)
            .ThenInclude(i => i.Lines)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (line == null)
        {
            return NotFound();
        }

        if (line.Invoice != null)
        {
            line.Invoice.Lines.Remove(line);
            RecalculateInvoiceTotals(line.Invoice);
        }

        _db.InvoiceLines.Remove(line);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateLineAsync(Guid? productId, string? customDescription, Guid invoiceId)
    {
        var invoiceExists = await _db.Invoices.AnyAsync(x => x.Id == invoiceId);
        if (!invoiceExists)
        {
            return BadRequest(new { message = "Invoice not found." });
        }

        if (!productId.HasValue && string.IsNullOrWhiteSpace(customDescription))
        {
            return BadRequest(new { message = "CustomDescription is required when ProductId is not provided." });
        }

        if (productId.HasValue)
        {
            var productExists = await _db.Products.AnyAsync(p => p.Id == productId.Value);
            if (!productExists)
            {
                return BadRequest(new { message = "Product not found." });
            }
        }

        return null;
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

    private static void RecalculateInvoiceTotals(Invoice invoice)
    {
        invoice.Subtotal = invoice.Lines.Sum(x => x.LineSubtotal);
        invoice.DiscountTotal = invoice.Lines.Sum(x => x.DiscountAmount);
        invoice.TaxTotal = invoice.Lines.Sum(x => x.TaxAmount);
        invoice.Total = invoice.Lines.Sum(x => x.LineTotalWithTax);
        invoice.UpdatedAt = DateTime.UtcNow;
    }

    private static InvoiceLineDto ToDto(InvoiceLine line) => new()
    {
        Id = line.Id,
        ProductId = line.ProductId,
        ProductSku = line.ProductSku,
        ProductName = line.ProductName,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        IsTaxInclusive = line.IsTaxInclusive,
        DiscountRate = line.DiscountRate,
        DiscountAmount = line.DiscountAmount,
        LineSubtotal = line.LineSubtotal,
        LineTotal = line.LineTotal,
        TaxRate = line.TaxRate,
        TaxAmount = line.TaxAmount,
        LineTotalWithTax = line.LineTotalWithTax,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote
    };
}
