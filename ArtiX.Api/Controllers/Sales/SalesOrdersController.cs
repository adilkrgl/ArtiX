using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/sales/[controller]")]
[Authorize(Roles = "Admin")] 
public class SalesOrdersController : ControllerBase
{
    private readonly ErpDbContext _db;

    private static readonly Expression<Func<SalesOrder, SalesOrderDto>> ToDtoExpression = order => new SalesOrderDto
    {
        Id = order.Id,
        CompanyId = order.CompanyId,
        BranchId = order.BranchId,
        CustomerId = order.CustomerId,
        SalesChannelId = order.SalesChannelId,
        SalesRepresentativeId = order.SalesRepresentativeId,
        OrderDate = order.OrderDate,
        Status = order.Status,
        TotalAmount = order.Lines.Sum(l => l.Quantity * l.UnitPrice),
        Lines = order.Lines.Select(l => new SalesOrderLineDto
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

    private static SalesOrderDto ToDto(SalesOrder order) => ToDtoExpression.Compile().Invoke(order);

    public SalesOrdersController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesOrderDto>>> GetAsync(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? customerId)
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

        var items = await query
            .Select(ToDtoExpression)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> GetByIdAsync(Guid id)
    {
        var order = await _db.SalesOrders
            .Where(x => x.Id == id)
            .Include(x => x.Lines)
            .Select(ToDtoExpression)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrderDto>> CreateAsync(
      [FromQuery] Guid? companyId,
      [FromBody] CreateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (companyId.HasValue && request.CompanyId != Guid.Empty && companyId.Value != request.CompanyId)
        {
            return BadRequest(new { message = "CompanyId mismatch between route and payload." });
        }

        var effectiveCompanyId = companyId.HasValue && companyId.Value != Guid.Empty
            ? companyId.Value
            : request.CompanyId;

        if (effectiveCompanyId == Guid.Empty)
        {
            return BadRequest(new { message = "CompanyId is required." });
        }

        var validation = await ValidateHeaderAsync(
            effectiveCompanyId,
            request.BranchId,
            request.CustomerId,
            request.SalesChannelId,
            request.SalesRepresentativeId);

        if (validation is ObjectResult error)
        {
            return error;
        }

        var entity = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = effectiveCompanyId,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            SalesChannelId = request.SalesChannelId,
            SalesRepresentativeId = request.SalesRepresentativeId,
            OrderDate = request.OrderDate ?? DateTime.UtcNow,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status,
            CreatedAt = DateTime.UtcNow,
            Lines = new List<SalesOrderLine>()
        };

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

        _db.SalesOrders.Add(entity);
        await _db.SaveChangesAsync();


        //return CreatedAtAction(
        //    nameof(GetByIdAsync),
        //    "SalesOrders",
        //    new { id = entity.Id },
        //    ToDto(entity));

        return Created($"/api/sales/SalesOrders/{entity.Id}", ToDto(entity));

    }


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesOrderDto>> UpdateAsync(Guid id, [FromBody] UpdateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var order = await _db.SalesOrders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
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

        if (request.Lines != null)
        {
            if (order.Lines.Any())
            {
                _db.SalesOrderLines.RemoveRange(order.Lines);
                order.Lines.Clear();
            }

            foreach (var lineRequest in request.Lines)
            {
                var line = new SalesOrderLine
                {
                    Id = Guid.NewGuid(),
                    SalesOrderId = order.Id,
                    ProductId = lineRequest.ProductId,
                    Quantity = lineRequest.Quantity,
                    UnitPrice = lineRequest.UnitPrice,
                    CustomDescription = lineRequest.CustomDescription,
                    LineNote = lineRequest.LineNote,
                    CompanyId = order.CompanyId,
                    BranchId = order.BranchId,
                    TenantId = order.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                order.Lines.Add(line);
            }
        }

        await _db.SaveChangesAsync();

        var dto = await _db.SalesOrders
            .Where(x => x.Id == order.Id)
            .Include(x => x.Lines)
            .Select(ToDtoExpression)
            .FirstAsync();

        return Ok(dto);
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
}
