using ArtiX.Api.Dtos.Products;
using ArtiX.Domain.Entities.Products;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Products;

[ApiController]
[Route("api/attribute-values")]
[Authorize(Roles = "Admin")]
public class AttributeValuesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public AttributeValuesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet("by-definition/{attributeDefinitionId:guid}")]
    public async Task<ActionResult<List<AttributeValueDto>>> GetByDefinition(Guid attributeDefinitionId)
    {
        var definitionExists = await _db.AttributeDefinitions.AnyAsync(x => x.Id == attributeDefinitionId);
        if (!definitionExists)
        {
            return NotFound();
        }

        var values = await _db.AttributeValues
            .Where(x => x.AttributeDefinitionId == attributeDefinitionId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Value)
            .Select(x => new AttributeValueDto
            {
                Id = x.Id,
                AttributeDefinitionId = x.AttributeDefinitionId,
                Value = x.Value,
                SortOrder = x.SortOrder
            })
            .ToListAsync();

        return Ok(values);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttributeValueDto>> Get(Guid id)
    {
        var value = await _db.AttributeValues.FirstOrDefaultAsync(x => x.Id == id);
        if (value == null)
        {
            return NotFound();
        }

        return Ok(ToDto(value));
    }

    [HttpPost]
    public async Task<ActionResult<AttributeValueDto>> Create(CreateAttributeValueRequest request)
    {
        var definition = await _db.AttributeDefinitions.FirstOrDefaultAsync(x => x.Id == request.AttributeDefinitionId);
        if (definition == null)
        {
            return BadRequest(new { message = "Attribute definition not found." });
        }

        var entity = new AttributeValue
        {
            Id = Guid.NewGuid(),
            AttributeDefinitionId = request.AttributeDefinitionId,
            Value = request.Value,
            SortOrder = request.SortOrder,
            TenantId = definition.TenantId,
            CompanyId = definition.CompanyId,
            BranchId = definition.BranchId,
            CreatedAt = DateTime.UtcNow
        };

        _db.AttributeValues.Add(entity);
        await _db.SaveChangesAsync();

        var dto = ToDto(entity);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AttributeValueDto>> Update(Guid id, UpdateAttributeValueRequest request)
    {
        var entity = await _db.AttributeValues.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Value = request.Value;
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.AttributeValues.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        var inUse = await _db.ProductAttributeValues.AnyAsync(x => x.AttributeValueId == id);
        if (inUse)
        {
            return BadRequest(new { message = "Cannot delete attribute value that is used by products." });
        }

        _db.AttributeValues.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static AttributeValueDto ToDto(AttributeValue value) => new()
    {
        Id = value.Id,
        AttributeDefinitionId = value.AttributeDefinitionId,
        Value = value.Value,
        SortOrder = value.SortOrder
    };
}
