using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtiX.Api.Dtos.Products;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Products;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Products;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly ErpDbContext _db;

    public ProductsController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? productTypeId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
    {
        var query = _db.Products.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId);
        }

        if (branchId.HasValue)
        {
            query = query.Where(p => p.BranchId == branchId);
        }

        if (productTypeId.HasValue)
        {
            query = query.Where(p => p.ProductTypeId == productTypeId);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.Sku != null && p.Sku.Contains(term)) ||
                (p.Barcode != null && p.Barcode.Contains(term)));
        }

        var results = await query
            .Select(p => ToDto(p))
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(ToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var company = await _db.Companies.FindAsync(request.CompanyId);
        if (company is null)
        {
            return BadRequest(new { message = "Company not found." });
        }

        Branch? branch = null;
        if (request.BranchId.HasValue)
        {
            branch = await _db.Branches.FindAsync(request.BranchId.Value);
            if (branch is null || branch.CompanyId != request.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to the specified company." });
            }
        }

        if (request.ProductTypeId.HasValue)
        {
            var productTypeExists = await _db.ProductTypes.AnyAsync(pt => pt.Id == request.ProductTypeId.Value);
            if (!productTypeExists)
            {
                return BadRequest(new { message = "Product type not found." });
            }
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            ProductTypeId = request.ProductTypeId,
            Name = request.Name,
            Sku = request.Sku,
            Barcode = request.Barcode,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var dto = ToDto(product);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _db.Branches.FindAsync(request.BranchId.Value);
            if (branch is null || branch.CompanyId != product.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to the product's company." });
            }
        }

        if (request.ProductTypeId.HasValue)
        {
            var productTypeExists = await _db.ProductTypes.AnyAsync(pt => pt.Id == request.ProductTypeId.Value);
            if (!productTypeExists)
            {
                return BadRequest(new { message = "Product type not found." });
            }
        }

        product.BranchId = request.BranchId;
        product.ProductTypeId = request.ProductTypeId;
        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ProductDto ToDto(Product entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        BranchId = entity.BranchId,
        ProductTypeId = entity.ProductTypeId,
        Name = entity.Name,
        Sku = entity.Sku,
        Barcode = entity.Barcode,
        IsActive = entity.IsActive
    };
}
