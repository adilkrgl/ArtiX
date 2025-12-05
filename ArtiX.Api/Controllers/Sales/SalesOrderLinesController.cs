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
public class SalesOrderLinesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public SalesOrderLinesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesOrderLineDto>>> GetAsync([FromQuery] Guid? salesOrderId)
    {
        var query = _db.SalesOrderLines.AsQueryable();

        if (salesOrderId.HasValue)
        {
            query = query.Where(x => x.SalesOrderId == salesOrderId.Value);
        }

        var items = await query
            .Select(x => new SalesOrderLineDto
            {
                Id = x.Id,
                SalesOrderId = x.SalesOrderId,
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
    public async Task<ActionResult<SalesOrderLineDto>> GetByIdAsync(Guid id)
    {
        var dto = await _db.SalesOrderLines
            .Where(x => x.Id == id)
            .Select(x => new SalesOrderLineDto
            {
                Id = x.Id,
                SalesOrderId = x.SalesOrderId,
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
    public async Task<ActionResult<SalesOrderLineDto>> CreateAsync([FromBody] CreateSalesOrderLineRequest request)
    {
        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, request.SalesOrderId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = request.SalesOrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CreatedAt = DateTime.UtcNow
        };

        _db.SalesOrderLines.Add(line);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = line.Id }, ToDto(line));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesOrderLineDto>> UpdateAsync(Guid id, [FromBody] UpdateSalesOrderLineRequest request)
    {
        var line = await _db.SalesOrderLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.CustomDescription, line.SalesOrderId);
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
        var line = await _db.SalesOrderLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        _db.SalesOrderLines.Remove(line);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateLineAsync(Guid? productId, string? customDescription, Guid salesOrderId)
    {
        var orderExists = await _db.SalesOrders.AnyAsync(x => x.Id == salesOrderId);
        if (!orderExists)
        {
            return BadRequest(new { message = "Sales order not found." });
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

    private static SalesOrderLineDto ToDto(SalesOrderLine line) => new()
    {
        Id = line.Id,
        SalesOrderId = line.SalesOrderId,
        ProductId = line.ProductId,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote
    };
}
