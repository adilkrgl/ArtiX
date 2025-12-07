using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/[controller]")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(GroupName = "Sales")]
public class SalesOrderLinesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public SalesOrderLinesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesOrderLineDto>>> GetAsync([FromQuery] Guid? salesOrderId, [FromQuery] Guid? companyId, [FromQuery] Guid? branchId)
    {
        var query = _db.SalesOrderLines.AsQueryable();

        if (salesOrderId.HasValue)
        {
            query = query.Where(x => x.SalesOrderId == salesOrderId.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        var items = await query
            .AsNoTracking()
            .Select(x => new SalesOrderLineDto
            {
                Id = x.Id,
                SalesOrderId = x.SalesOrderId,
                ProductId = x.ProductId ?? Guid.Empty,
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
                ProductId = x.ProductId ?? Guid.Empty,
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
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validationResult = await ValidateLineAsync(request.ProductId, request.SalesOrderId);
        if (validationResult is ObjectResult errorResult)
        {
            return errorResult;
        }

        var parentOrder = await _db.SalesOrders.FirstAsync(x => x.Id == request.SalesOrderId);

        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = request.SalesOrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            CustomDescription = request.CustomDescription,
            LineNote = request.LineNote,
            CompanyId = parentOrder.CompanyId,
            BranchId = parentOrder.BranchId,
            TenantId = parentOrder.TenantId,
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

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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

    private async Task<IActionResult?> ValidateLineAsync(Guid productId, Guid salesOrderId)
    {
        var orderExists = await _db.SalesOrders.AnyAsync(x => x.Id == salesOrderId);
        if (!orderExists)
        {
            return BadRequest(new { message = "SalesOrder not found" });
        }

        var productExists = await _db.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return BadRequest(new { message = "Product not found" });
        }

        return null;
    }

    private static SalesOrderLineDto ToDto(SalesOrderLine line) => new()
    {
        Id = line.Id,
        SalesOrderId = line.SalesOrderId,
        ProductId = line.ProductId ?? Guid.Empty,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        CustomDescription = line.CustomDescription,
        LineNote = line.LineNote
    };
}
