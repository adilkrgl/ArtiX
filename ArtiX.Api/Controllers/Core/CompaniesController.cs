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
public class CompaniesController : ControllerBase
{
    private readonly ErpDbContext _context;

    public CompaniesController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies([FromQuery] string? search, [FromQuery] Guid? tenantId)
    {
        var query = _context.Companies.AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(c => c.TenantId == tenantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) || c.Code.ToLower().Contains(term));
        }

        var items = await query
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
    {
        var entity = await _context.Companies.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(ToDto(entity));
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = new Company
        {
            Name = request.Name,
            Code = request.Code,
            TaxNumber = request.TaxNumber,
            TenantId = request.TenantId
        };

        _context.Companies.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCompany), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await _context.Companies.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.TaxNumber = request.TaxNumber;
        entity.TenantId = request.TenantId;

        await _context.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var entity = await _context.Companies.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.Companies.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static CompanyDto ToDto(Company entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Code = entity.Code,
        TaxNumber = entity.TaxNumber,
        TenantId = entity.TenantId
    };
}
