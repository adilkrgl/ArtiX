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
public class SalesOrdersController : ControllerBase
{
    private readonly ErpDbContext _db;

    public SalesOrdersController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesOrderDto>>> GetAsync([FromQuery] Guid? companyId, [FromQuery] Guid? branchId, [FromQuery] Guid? customerId)
    {
        var query = _db.SalesOrders
            .Include(x => x.Lines)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        var items = await query.AsNoTracking().ToListAsync();
        return Ok(items.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> GetByIdAsync(Guid id)
    {
        var order = await _db.SalesOrders
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrderDto>> CreateAsync([FromBody] CreateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validation = await ValidateHeaderAsync(request.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        var entity = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            SalesChannelId = request.SalesChannelId,
            SalesRepresentativeId = request.SalesRepresentativeId,
            OrderDate = request.OrderDate ?? DateTime.UtcNow,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status,
            CreatedAt = DateTime.UtcNow
        };

        _db.SalesOrders.Add(entity);

        if (request.Lines != null && request.Lines.Any())
        {
            foreach (var lineRequest in request.Lines)
            {
                var line = new SalesOrderLine
                {
                    Id = Guid.NewGuid(),
                    SalesOrder = entity,
                    SalesOrderId = entity.Id,
                    ProductId = lineRequest.ProductId,
                    Quantity = lineRequest.Quantity,
                    UnitPrice = lineRequest.UnitPrice,
                    CustomDescription = lineRequest.CustomDescription,
                    LineNote = lineRequest.LineNote,
                    CompanyId = entity.CompanyId,
                    BranchId = entity.BranchId,
                    TenantId = entity.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                entity.Lines.Add(line);
            }
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> UpdateAsync(Guid id, [FromBody] UpdateSalesOrderRequest request)
    {
        var order = await _db.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validation = await ValidateHeaderAsync(order.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        order.BranchId = request.BranchId;
        order.CustomerId = request.CustomerId;
        order.SalesChannelId = request.SalesChannelId;
        order.SalesRepresentativeId = request.SalesRepresentativeId;
        order.OrderDate = request.OrderDate ?? order.OrderDate;
        order.Status = string.IsNullOrWhiteSpace(request.Status) ? order.Status : request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var reloaded = await _db.SalesOrders
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstAsync(x => x.Id == order.Id);

        return Ok(ToDto(reloaded));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var order = await _db.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (order.Lines.Any())
        {
            _db.SalesOrderLines.RemoveRange(order.Lines);
        }

        _db.SalesOrders.Remove(order);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateHeaderAsync(Guid companyId, Guid? branchId, Guid? customerId, Guid? salesChannelId, Guid? salesRepresentativeId)
    {
        if (companyId == Guid.Empty)
        {
            return BadRequest(new { message = "CompanyId is required." });
        }

        var companyExists = await _db.Companies.AnyAsync(x => x.Id == companyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (branchId.HasValue)
        {
            var branchValid = await _db.Branches.AnyAsync(x => x.Id == branchId.Value && x.CompanyId == companyId);
            if (!branchValid)
            {
                return BadRequest(new { message = "Branch not found for the given company." });
            }
        }

        if (customerId.HasValue)
        {
            var customerValid = await _db.Customers.AnyAsync(x =>
                x.Id == customerId.Value &&
                x.CompanyId == companyId &&
                (!branchId.HasValue || x.BranchId == null || x.BranchId == branchId));
            if (!customerValid)
            {
                return BadRequest(new { message = "Customer not found for the given company." });
            }
        }

        if (salesChannelId.HasValue)
        {
            var channelValid = await _db.SalesChannels.AnyAsync(x => x.Id == salesChannelId.Value && x.CompanyId == companyId);
            if (!channelValid)
            {
                return BadRequest(new { message = "Sales channel not found for the given company." });
            }
        }

        if (salesRepresentativeId.HasValue)
        {
            var repValid = await _db.SalesRepresentatives.AnyAsync(x => x.Id == salesRepresentativeId.Value && x.CompanyId == companyId);
            if (!repValid)
            {
                return BadRequest(new { message = "Sales representative not found for the given company." });
            }
        }

        return null;
    }

    private static SalesOrderDto ToDto(SalesOrder order)
    {
        var lines = order.Lines ?? Enumerable.Empty<SalesOrderLine>();

        return new SalesOrderDto
        {
            Id = order.Id,
            CompanyId = order.CompanyId,
            BranchId = order.BranchId,
            CustomerId = order.CustomerId,
            SalesChannelId = order.SalesChannelId,
            SalesRepresentativeId = order.SalesRepresentativeId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = lines.Sum(l => l.Quantity * l.UnitPrice),
            Lines = lines.Select(l => new SalesOrderLineDto
            {
                Id = l.Id,
                SalesOrderId = l.SalesOrderId,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                CustomDescription = l.CustomDescription,
                LineNote = l.LineNote
            }).ToList()
        };
    }
}
