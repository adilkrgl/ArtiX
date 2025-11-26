using ArtiX.Api.Dtos.Products;
using ArtiX.Domain.Entities.Products;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Products;

[ApiController]
[Route("api/products/{productId:guid}/attributes")]
[Authorize(Roles = "Admin")]
public class ProductAttributesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public ProductAttributesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductAttributeValueDto>>> Get(Guid productId)
    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return NotFound();
        }

        var items = await _db.ProductAttributeValues
            .Include(pav => pav.AttributeValue)
            .ThenInclude(av => av.AttributeDefinition)
            .Where(pav => pav.ProductId == productId)
            .Select(ToDto)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPut]
    public async Task<ActionResult<List<ProductAttributeValueDto>>> Upsert(Guid productId, [FromBody] List<UpsertProductAttributeRequest> requests)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
        {
            return NotFound();
        }

        var definitionIds = requests.Select(r => r.AttributeDefinitionId).Distinct().ToList();
        var definitions = await _db.AttributeDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id);

        if (definitions.Count != definitionIds.Count)
        {
            return BadRequest(new { message = "One or more attribute definitions were not found." });
        }

        var valueIds = requests.Where(r => r.AttributeValueId.HasValue).Select(r => r.AttributeValueId!.Value).Distinct().ToList();
        var values = await _db.AttributeValues
            .Include(v => v.AttributeDefinition)
            .Where(v => valueIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        foreach (var request in requests)
        {
            if (request.AttributeValueId == null)
            {
                return BadRequest(new { message = "AttributeValueId is required for each attribute." });
            }

            if (!values.TryGetValue(request.AttributeValueId.Value, out var attributeValue))
            {
                return BadRequest(new { message = "One or more attribute values were not found." });
            }

            if (attributeValue.AttributeDefinitionId != request.AttributeDefinitionId)
            {
                return BadRequest(new { message = "Attribute value does not belong to the specified definition." });
            }
        }

        var existing = await _db.ProductAttributeValues
            .Where(pav => pav.ProductId == productId)
            .ToListAsync();

        var toKeep = new List<ProductAttributeValue>();

        foreach (var request in requests)
        {
            var match = existing.FirstOrDefault(x => x.AttributeValueId == request.AttributeValueId.Value);

            if (match == null)
            {
                match = new ProductAttributeValue
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    AttributeValueId = request.AttributeValueId.Value,
                    CustomValue = request.CustomValue,
                    TenantId = product.TenantId,
                    CompanyId = product.CompanyId,
                    BranchId = product.BranchId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.ProductAttributeValues.Add(match);
            }
            else
            {
                match.CustomValue = request.CustomValue;
                match.UpdatedAt = DateTime.UtcNow;
            }

            toKeep.Add(match);
        }

        var toRemove = existing.Where(x => !toKeep.Contains(x)).ToList();
        if (toRemove.Count > 0)
        {
            _db.ProductAttributeValues.RemoveRange(toRemove);
        }

        await _db.SaveChangesAsync();

        var result = await _db.ProductAttributeValues
            .Include(pav => pav.AttributeValue)
            .ThenInclude(av => av.AttributeDefinition)
            .Where(pav => pav.ProductId == productId)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    private static ProductAttributeValueDto ToDto(ProductAttributeValue pav) => new()
    {
        Id = pav.Id,
        ProductId = pav.ProductId,
        AttributeDefinitionId = pav.AttributeValue.AttributeDefinitionId,
        AttributeValueId = pav.AttributeValueId,
        AttributeName = pav.AttributeValue.AttributeDefinition.Name,
        AttributeDisplayName = pav.AttributeValue.AttributeDefinition.DisplayName ?? pav.AttributeValue.AttributeDefinition.Name,
        AttributeValue = pav.AttributeValue.Value,
        CustomValue = pav.CustomValue
    };
}
