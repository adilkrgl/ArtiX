using ArtiX.Api.Dtos.Products;
using ArtiX.Domain.Entities.Products;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Products;

[ApiController]
[Route("api/attribute-definitions")]
[Authorize(Roles = "Admin")]
public class AttributeDefinitionsController : ControllerBase
{
    private readonly ErpDbContext _db;

    public AttributeDefinitionsController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AttributeDefinitionDto>>> GetList(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        [FromQuery] bool? isVariant,
        [FromQuery] bool? isFilterable,
        [FromQuery] string? search)
    {
        var query = _db.AttributeDefinitions.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId);
        }

        if (isVariant.HasValue)
        {
            query = query.Where(x => x.IsVariant == isVariant);
        }

        if (isFilterable.HasValue)
        {
            query = query.Where(x => x.IsFilterable == isFilterable);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.DisplayName != null && x.DisplayName.Contains(term)));
        }

        var results = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(ToDto)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttributeDefinitionDto>> Get(Guid id)
    {
        var definition = await _db.AttributeDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (definition == null)
        {
            return NotFound();
        }

        return Ok(ToDto(definition));
    }

    [HttpPost]
    public async Task<ActionResult<AttributeDefinitionDto>> Create(CreateAttributeDefinitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        if (request.CompanyId.HasValue)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.Id == request.CompanyId.Value);
            if (!companyExists)
            {
                return BadRequest(new { message = "Company not found." });
            }
        }

        var entity = new AttributeDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            Name = request.Name,
            DisplayName = request.DisplayName,
            DataType = (Domain.Enums.AttributeDataType)request.DataType,
            IsVariant = request.IsVariant,
            IsFilterable = request.IsFilterable,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _db.AttributeDefinitions.Add(entity);
        await _db.SaveChangesAsync();

        var dto = ToDto(entity);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AttributeDefinitionDto>> Update(Guid id, UpdateAttributeDefinitionRequest request)
    {
        var entity = await _db.AttributeDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.DisplayName = request.DisplayName;
        entity.DataType = (Domain.Enums.AttributeDataType)request.DataType;
        entity.IsVariant = request.IsVariant;
        entity.IsFilterable = request.IsFilterable;
        entity.IsRequired = request.IsRequired;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.AttributeDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        var inUse = await _db.AttributeValues.AnyAsync(v => v.AttributeDefinitionId == id)
                   || await _db.ProductAttributeValues.AnyAsync(pav => pav.AttributeValue.AttributeDefinitionId == id);

        if (inUse)
        {
            return BadRequest(new { message = "Cannot delete attribute definition that is in use." });
        }

        _db.AttributeDefinitions.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static AttributeDefinitionDto ToDto(AttributeDefinition def) => new()
    {
        Id = def.Id,
        CompanyId = def.CompanyId,
        BranchId = def.BranchId,
        Name = def.Name,
        DisplayName = def.DisplayName,
        DataType = (int)def.DataType,
        IsVariant = def.IsVariant,
        IsFilterable = def.IsFilterable,
        IsRequired = def.IsRequired,
        SortOrder = def.SortOrder
    };
}
