using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SalesRepresentativesController : ControllerBase
{
    private readonly ErpDbContext _context;

    public SalesRepresentativesController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesRepresentativeDto>>> Get([FromQuery] Guid? companyId, [FromQuery] string? search)
    {
        var query = _context.SalesRepresentatives.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(term)
                                     || (x.Email ?? string.Empty).ToLower().Contains(term)
                                     || (x.Phone ?? string.Empty).ToLower().Contains(term));
        }

        var reps = await query.AsNoTracking().ToListAsync();
        return reps.Select(ToDto).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesRepresentativeDto>> Get(Guid id)
    {
        var rep = await _context.SalesRepresentatives.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (rep == null)
        {
            return NotFound();
        }

        return ToDto(rep);
    }

    [HttpPost]
    public async Task<ActionResult<SalesRepresentativeDto>> Post(CreateSalesRepresentativeRequest request)
    {
        var companyExists = await _context.Companies.AnyAsync(x => x.Id == request.CompanyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        var entity = new SalesRepresentative
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SalesRepresentatives.AddAsync(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SalesRepresentativeDto>> Put(Guid id, UpdateSalesRepresentativeRequest request)
    {
        var entity = await _context.SalesRepresentatives.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.SalesRepresentatives.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.SalesRepresentatives.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static SalesRepresentativeDto ToDto(SalesRepresentative entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        FullName = entity.FullName,
        Email = entity.Email,
        Phone = entity.Phone
    };
}
