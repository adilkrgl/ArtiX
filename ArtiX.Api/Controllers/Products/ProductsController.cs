using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

    private static readonly Expression<Func<Product, ProductDto>> ToDtoExpression = p => new ProductDto
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        BranchId = p.BranchId,
        ProductTypeId = p.ProductTypeId,
        ManufacturerId = p.ManufacturerId,
        ManufacturerName = p.Manufacturer == null ? null : p.Manufacturer.Name,
        ManufacturerCode = p.Manufacturer == null ? null : p.Manufacturer.Code,
        Name = p.Name,
        Sku = p.Sku,
        Barcode = p.Barcode,
        CostPrice = p.CostPrice,
        RetailPrice = p.RetailPrice,
        WholesalePrice = p.WholesalePrice,
        TaxRate =p.TaxRate,
        IsActive = p.IsActive,
        Manufacturer = p.Manufacturer == null
            ? null
            : new ManufacturerDto
            {
                Id = p.Manufacturer.Id,
                CompanyId = p.Manufacturer.CompanyId,
                BranchId = p.Manufacturer.BranchId,
                Name = p.Manufacturer.Name,
                Code = p.Manufacturer.Code,
                ProductNameAtManufacturer = p.Manufacturer.ProductNameAtManufacturer,
                Address = p.Manufacturer.Address,
                Phone = p.Manufacturer.Phone,
                Website = p.Manufacturer.Website,
                ContactPerson = p.Manufacturer.ContactPerson
            }
    };

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
            .Select(ToDtoExpression)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _db.Products
            .Where(p => p.Id == id)
            .Select(ToDtoExpression)
            .FirstOrDefaultAsync();
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
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

        Manufacturer? manufacturer = null;
        if (request.ManufacturerId.HasValue)
        {
            manufacturer = await _db.Manufacturers.FindAsync(request.ManufacturerId.Value);
            if (manufacturer is null || manufacturer.CompanyId != request.CompanyId)
            {
                return BadRequest(new { message = "Manufacturer not found or does not belong to the specified company." });
            }
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            ProductTypeId = request.ProductTypeId,
            ManufacturerId = request.ManufacturerId,
            Name = request.Name,
            Sku = request.Sku,
            Barcode = request.Barcode,
            CostPrice = request.CostPrice,
            RetailPrice = request.RetailPrice,
            WholesalePrice = request.WholesalePrice,
            TaxRate = request.TaxRate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        if (manufacturer is not null)
        {
            product.Manufacturer = manufacturer;
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var dto = ToDto(product);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _db.Products
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.Id == id);
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

        Manufacturer? manufacturer = null;
        if (request.ManufacturerId.HasValue)
        {
            manufacturer = await _db.Manufacturers.FindAsync(request.ManufacturerId.Value);
            if (manufacturer is null || manufacturer.CompanyId != product.CompanyId)
            {
                return BadRequest(new { message = "Manufacturer not found or does not belong to the product's company." });
            }
        }

        product.BranchId = request.BranchId;
        product.ProductTypeId = request.ProductTypeId;
        product.ManufacturerId = request.ManufacturerId;
        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.CostPrice = request.CostPrice;
        product.RetailPrice = request.RetailPrice;
        product.WholesalePrice = request.WholesalePrice;
        product.TaxRate = request.TaxRate;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        product.Manufacturer = manufacturer;

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
        ManufacturerId = entity.ManufacturerId,
        ManufacturerName = entity.Manufacturer?.Name,
        ManufacturerCode = entity.Manufacturer?.Code,
        Name = entity.Name,
        Sku = entity.Sku,
        Barcode = entity.Barcode,
        CostPrice = entity.CostPrice,
        RetailPrice = entity.RetailPrice,
        WholesalePrice = entity.WholesalePrice,
        TaxRate=entity.TaxRate,
        IsActive = entity.IsActive,
        Manufacturer = entity.Manufacturer is null ? null : new ManufacturerDto
        {
            Id = entity.Manufacturer.Id,
            CompanyId = entity.Manufacturer.CompanyId,
            BranchId = entity.Manufacturer.BranchId,
            Name = entity.Manufacturer.Name,
            ProductNameAtManufacturer = entity.Manufacturer.ProductNameAtManufacturer,
            Address = entity.Manufacturer.Address,
            Phone = entity.Manufacturer.Phone,
            Website = entity.Manufacturer.Website,
            ContactPerson = entity.Manufacturer.ContactPerson
        }
    };
}
