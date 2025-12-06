using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                InvoiceId = x.InvoiceId,
                ProductId = x.ProductId,
                ProductSku = x.ProductSku,
                ProductName = x.ProductName,
                CustomDescription = x.CustomDescription,
                LineNote = x.LineNote,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountRate = x.DiscountRate,
                DiscountAmount = x.DiscountAmount,
                LineSubtotal = x.LineSubtotal,
                LineTotal = x.LineTotal,
                TaxRate = x.TaxRate,
                TaxAmount = x.TaxAmount,
                LineTotalWithTax = x.LineTotalWithTax
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
                InvoiceId = x.InvoiceId,
                ProductId = x.ProductId,
                ProductSku = x.ProductSku,
                ProductName = x.ProductName,
                CustomDescription = x.CustomDescription,
                LineNote = x.LineNote,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountRate = x.DiscountRate,
                DiscountAmount = x.DiscountAmount,
                LineSubtotal = x.LineSubtotal,
                LineTotal = x.LineTotal,
                TaxRate = x.TaxRate,
                TaxAmount = x.TaxAmount,
                LineTotalWithTax = x.LineTotalWithTax
            })
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            return NotFound();
        }

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceLineDto>> CreateAsync([FromBody] CreateInvoiceLineRequest request)
    {
        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, request.InvoiceId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

        if (invoice == null)
        {
            return BadRequest(new { message = "Invoice not found." });
        }

        Product? product = null;
        if (request.ProductId.HasValue)
        {
            product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId.Value);
        }

        var unitPrice = product?.RetailPrice ?? 0m;
        var taxRate = product?.TaxRate ?? 0m;
        var lineSubtotal = request.Quantity * unitPrice;
        var discountAmount = (request.DiscountRate / 100m) * lineSubtotal;
        var lineTotal = lineSubtotal - discountAmount;
        var taxAmount = (taxRate / 100m) * lineTotal;
        var lineTotalWithTax = lineTotal + taxAmount;

        var line = new InvoiceLine
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            ProductId = product?.Id,
            Product = product,
            ProductSku = product?.Sku,
            ProductName = product?.Name,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            DiscountRate = request.DiscountRate,
            DiscountAmount = discountAmount,
            LineSubtotal = lineSubtotal,
            LineTotal = lineTotal,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            LineTotalWithTax = lineTotalWithTax,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CreatedAt = DateTime.UtcNow
        };

        invoice.Lines.Add(line);

        UpdateTotals(invoice);

        await _db.SaveChangesAsync();

        return Ok(ToDto(line));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceLineDto>> UpdateAsync(Guid id, [FromBody] UpdateInvoiceLineRequest request)
    {
        var line = await _db.InvoiceLines
            .Include(l => l.Invoice)
            .ThenInclude(i => i!.Lines)
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

        var unitPrice = product?.RetailPrice ?? 0m;
        var taxRate = product?.TaxRate ?? 0m;
        var lineSubtotal = request.Quantity * unitPrice;
        var discountAmount = (request.DiscountRate / 100m) * lineSubtotal;
        var lineTotal = lineSubtotal - discountAmount;
        var taxAmount = (taxRate / 100m) * lineTotal;
        var lineTotalWithTax = lineTotal + taxAmount;

        line.ProductId = product?.Id;
        line.Product = product;
        line.ProductSku = product?.Sku;
        line.ProductName = product?.Name;
        line.Quantity = request.Quantity;
        line.UnitPrice = unitPrice;
        line.DiscountRate = request.DiscountRate;
        line.DiscountAmount = discountAmount;
        line.LineSubtotal = lineSubtotal;
        line.LineTotal = lineTotal;
        line.TaxRate = taxRate;
        line.TaxAmount = taxAmount;
        line.LineTotalWithTax = lineTotalWithTax;
        line.CustomDescription = request.CustomDescription;
        line.LineNote = request.LineNote;
        line.UpdatedAt = DateTime.UtcNow;

        if (line.Invoice != null)
        {
            UpdateTotals(line.Invoice);
        }

        await _db.SaveChangesAsync();

        return Ok(ToDto(line));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var line = await _db.InvoiceLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        _db.InvoiceLines.Remove(line);
        await _db.SaveChangesAsync();

        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == line.InvoiceId);

        if (invoice != null)
        {
            UpdateTotals(invoice);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private static void UpdateTotals(Invoice invoice)
    {
        var lines = invoice.Lines ?? Enumerable.Empty<InvoiceLine>();

        invoice.Subtotal = lines.Sum(x => x.LineSubtotal);
        invoice.DiscountTotal = lines.Sum(x => x.DiscountAmount);
        invoice.TaxTotal = lines.Sum(x => x.TaxAmount);
        invoice.Total = lines.Sum(x => x.LineTotalWithTax);
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

    private static InvoiceLineDto ToDto(InvoiceLine line) => new()
    {
        Id = line.Id,
        InvoiceId = line.InvoiceId,
        ProductId = line.ProductId,
        ProductSku = line.ProductSku,
        ProductName = line.ProductName,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        DiscountRate = line.DiscountRate,
        DiscountAmount = line.DiscountAmount,
        LineSubtotal = line.LineSubtotal,
        LineTotal = line.LineTotal,
        TaxRate = line.TaxRate,
        TaxAmount = line.TaxAmount,
        LineTotalWithTax = line.LineTotalWithTax
    };
}
