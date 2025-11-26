using ArtiX.Api.Dtos.Core;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Enums;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BranchesController : ControllerBase
{
    private readonly ErpDbContext _context;

    public BranchesController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranches([FromQuery] Guid? companyId, [FromQuery] string? search)
    {
        var query = _context.Branches.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(b => b.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(term) || b.Code.ToLower().Contains(term));
        }

        var items = await query.Select(b => ToDto(b)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BranchDto>> GetBranch(Guid id)
    {
        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(ToDto(entity));
    }

    [HttpPost]
    public async Task<ActionResult<BranchDto>> CreateBranch([FromBody] CreateBranchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (!Enum.IsDefined(typeof(BranchType), request.BranchType))
        {
            return BadRequest(new { message = "Invalid branch type." });
        }

        var entity = new Branch
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Code = request.Code,
            BranchType = (BranchType)request.BranchType
        };

        _context.Branches.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBranch), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BranchDto>> UpdateBranch(Guid id, [FromBody] UpdateBranchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        if (!Enum.IsDefined(typeof(BranchType), request.BranchType))
        {
            return BadRequest(new { message = "Invalid branch type." });
        }

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.BranchType = (BranchType)request.BranchType;

        await _context.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.Branches.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static BranchDto ToDto(Branch entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        Name = entity.Name,
        Code = entity.Code,
        BranchType = (int)entity.BranchType
    };
}
