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
                InvoiceId = x.InvoiceId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
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
                InvoiceId = x.InvoiceId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
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
    public async Task<ActionResult<InvoiceLineDto>> CreateAsync([FromBody] CreateInvoiceLineRequest request)
    {
        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, request.InvoiceId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        var line = new InvoiceLine
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CreatedAt = DateTime.UtcNow
        };

        _db.InvoiceLines.Add(line);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = line.Id }, ToDto(line));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceLineDto>> UpdateAsync(Guid id, [FromBody] UpdateInvoiceLineRequest request)
    {
        var line = await _db.InvoiceLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, line.InvoiceId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        line.ProductId = request.ProductId;
        line.Quantity = request.Quantity;
        line.UnitPrice = request.UnitPrice;
        line.CustomDescription = request.CustomDescription;
        line.LineNote = request.LineNote;
        line.UpdatedAt = DateTime.UtcNow;

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

    private static InvoiceLineDto ToDto(InvoiceLine line) => new()
    {
        Id = line.Id,
        InvoiceId = line.InvoiceId,
        ProductId = line.ProductId,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote
    };
}
