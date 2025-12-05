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
public class QuotationLinesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public QuotationLinesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuotationLineDto>>> GetAsync([FromQuery] Guid? quotationId)
    {
        var query = _db.QuotationLines.AsQueryable();

        if (quotationId.HasValue)
        {
            query = query.Where(x => x.QuotationId == quotationId.Value);
        }

        var items = await query
            .Select(x => new QuotationLineDto
            {
                Id = x.Id,
                QuotationId = x.QuotationId,
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
    public async Task<ActionResult<QuotationLineDto>> GetByIdAsync(Guid id)
    {
        var dto = await _db.QuotationLines
            .Where(x => x.Id == id)
            .Select(x => new QuotationLineDto
            {
                Id = x.Id,
                QuotationId = x.QuotationId,
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
    public async Task<ActionResult<QuotationLineDto>> CreateAsync([FromBody] CreateQuotationLineRequest request)
    {
        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, request.QuotationId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        var line = new QuotationLine
        {
            Id = Guid.NewGuid(),
            QuotationId = request.QuotationId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CreatedAt = DateTime.UtcNow
        };

        _db.QuotationLines.Add(line);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = line.Id }, ToDto(line));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuotationLineDto>> UpdateAsync(Guid id, [FromBody] UpdateQuotationLineRequest request)
    {
        var line = await _db.QuotationLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, line.QuotationId);
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
        var line = await _db.QuotationLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        _db.QuotationLines.Remove(line);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateLineAsync(Guid? productId, string? customDescription, Guid quotationId)
    {
        var quotationExists = await _db.Quotations.AnyAsync(x => x.Id == quotationId);
        if (!quotationExists)
        {
            return BadRequest(new { message = "Quotation not found." });
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

    private static QuotationLineDto ToDto(QuotationLine line) => new()
    {
        Id = line.Id,
        QuotationId = line.QuotationId,
        ProductId = line.ProductId,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote
    };
}
