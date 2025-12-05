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
public class InvoicesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public InvoicesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetAsync([FromQuery] Guid? companyId, [FromQuery] Guid? branchId, [FromQuery] Guid? customerId)
    {
        var query = _db.Invoices
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
        return items.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetByIdAsync(Guid id)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        return ToDto(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateAsync([FromBody] CreateInvoiceRequest request)
    {
        var validation = await ValidateHeaderAsync(request.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            SalesChannelId = request.SalesChannelId,
            SalesRepresentativeId = request.SalesRepresentativeId,
            InvoiceDate = request.InvoiceDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync();

        // reload with lines (none yet) for consistent response
        entity.Lines = new List<InvoiceLine>();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> UpdateAsync(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        var validation = await ValidateHeaderAsync(invoice.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        invoice.BranchId = request.BranchId;
        invoice.CustomerId = request.CustomerId;
        invoice.SalesChannelId = request.SalesChannelId;
        invoice.SalesRepresentativeId = request.SalesRepresentativeId;
        invoice.InvoiceDate = request.InvoiceDate;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToDto(invoice);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        if (invoice.Lines.Any())
        {
            return BadRequest(new { message = "Cannot delete invoice with existing lines." });
        }

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateHeaderAsync(Guid companyId, Guid? branchId, Guid? customerId, Guid? salesChannelId, Guid? salesRepresentativeId)
    {
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
            var customerValid = await _db.Customers.AnyAsync(x => x.Id == customerId.Value && x.CompanyId == companyId);
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

    private static InvoiceDto ToDto(Invoice invoice)
    {
        var total = invoice.Lines?.Sum(x => x.Quantity * x.UnitPrice);

        return new InvoiceDto
        {
            Id = invoice.Id,
            CompanyId = invoice.CompanyId,
            BranchId = invoice.BranchId,
            CustomerId = invoice.CustomerId,
            SalesChannelId = invoice.SalesChannelId,
            SalesRepresentativeId = invoice.SalesRepresentativeId,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = total,
            Lines = invoice.Lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                InvoiceId = l.InvoiceId,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                CustomDescription = l.CustomDescription,
                LineNote = l.LineNote
            }).ToList()
        };
    }
}
