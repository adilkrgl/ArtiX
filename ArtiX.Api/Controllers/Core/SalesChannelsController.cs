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
public class SalesChannelsController : ControllerBase
{
    private readonly ErpDbContext _context;

    public SalesChannelsController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesChannelDto>>> GetSalesChannels(
        [FromQuery] Guid? companyId,
        [FromQuery] bool? isOnline,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
    {
        var query = _context.SalesChannels.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(sc => sc.CompanyId == companyId.Value);
        }

        if (isOnline.HasValue)
        {
            query = query.Where(sc => sc.IsOnline == isOnline.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(sc => sc.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(sc => sc.Name.ToLower().Contains(term) || sc.Code.ToLower().Contains(term));
        }

        var items = await query.Select(sc => ToDto(sc)).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesChannelDto>> GetSalesChannel(Guid id)
    {
        var entity = await _context.SalesChannels.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(ToDto(entity));
    }

    [HttpPost]
    public async Task<ActionResult<SalesChannelDto>> CreateSalesChannel([FromBody] CreateSalesChannelRequest request)
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

        if (!Enum.IsDefined(typeof(SalesChannelType), request.ChannelType))
        {
            return BadRequest(new { message = "Invalid channel type." });
        }

        var entity = new SalesChannel
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Code = request.Code,
            ChannelType = (SalesChannelType)request.ChannelType,
            ExternalSystem = request.ExternalSystem,
            IsOnline = request.IsOnline,
            IsActive = request.IsActive
        };

        _context.SalesChannels.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSalesChannel), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SalesChannelDto>> UpdateSalesChannel(Guid id, [FromBody] UpdateSalesChannelRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await _context.SalesChannels.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        if (!Enum.IsDefined(typeof(SalesChannelType), request.ChannelType))
        {
            return BadRequest(new { message = "Invalid channel type." });
        }

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.ChannelType = (SalesChannelType)request.ChannelType;
        entity.ExternalSystem = request.ExternalSystem;
        entity.IsOnline = request.IsOnline;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalesChannel(Guid id)
    {
        var entity = await _context.SalesChannels.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.SalesChannels.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static SalesChannelDto ToDto(SalesChannel entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        Name = entity.Name,
        Code = entity.Code,
        ChannelType = (int)entity.ChannelType,
        ExternalSystem = entity.ExternalSystem,
        IsOnline = entity.IsOnline,
        IsActive = entity.IsActive
    };
}
