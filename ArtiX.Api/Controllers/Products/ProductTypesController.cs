using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtiX.Api.Dtos.Products;
using ArtiX.Domain.Entities.Products;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Products;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductTypesController : ControllerBase
{
    private readonly ErpDbContext _db;

    public ProductTypesController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductTypeDto>>> GetAll([FromQuery] string? search)
    {
        var query = _db.ProductTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(pt => pt.Name.Contains(term));
        }

        var results = await query
            .Select(pt => ToDto(pt))
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductTypeDto>> GetById(Guid id)
    {
        var productType = await _db.ProductTypes.FindAsync(id);
        if (productType is null)
        {
            return NotFound();
        }

        return Ok(ToDto(productType));
    }

    [HttpPost]
    public async Task<ActionResult<ProductTypeDto>> Create([FromBody] CreateProductTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        var entity = new ProductType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductTypes.Add(entity);
        await _db.SaveChangesAsync();

        var dto = ToDto(entity);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductTypeDto>> Update(Guid id, [FromBody] UpdateProductTypeRequest request)
    {
        var entity = await _db.ProductTypes.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), "Name is required.");
            return ValidationProblem(ModelState);
        }

        entity.Name = request.Name.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.ProductTypes.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        _db.ProductTypes.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ProductTypeDto ToDto(ProductType entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };
}
