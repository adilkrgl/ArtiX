using ArtiX.Api.Dtos.Core;
using ArtiX.Domain.Entities.Core;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TenantsController : ControllerBase
{
    private readonly ErpDbContext _context;

    public TenantsController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenants([FromQuery] string? search)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(term) || (t.Code != null && t.Code.ToLower().Contains(term)));
        }

        var items = await query
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        return Ok(ToDto(tenant));
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = new Tenant
        {
            Name = request.Name,
            Code = request.Code
        };

        _context.Tenants.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenant), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await _context.Tenants.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Code = request.Code;

        await _context.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var entity = await _context.Tenants.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.Tenants.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TenantDto ToDto(Tenant entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Code = entity.Code
    };
}
