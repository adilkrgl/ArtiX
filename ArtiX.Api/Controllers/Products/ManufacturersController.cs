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
[Route("api/products/[controller]")]
[Authorize(Roles = "Admin")] 
public class ManufacturersController : ControllerBase
{
    private readonly ErpDbContext _db;

    public ManufacturersController(ErpDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManufacturerDto>>> GetAll([FromQuery] Guid? companyId, [FromQuery] Guid? branchId)
    {
        var query = _db.Manufacturers.AsNoTracking().AsQueryable();       

        if (companyId.HasValue)
        {
            query = query.Where(m => m.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(m => m.BranchId == branchId.Value);
        }

        var results = await query
            .OrderBy(m => m.Name)
            .Select(ToDtoExpression)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ManufacturerDto>> GetById(Guid id)
    {
        var manufacturer = await _db.Manufacturers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manufacturer is null)
        {
            return NotFound();
        }

        return Ok(ToDto(manufacturer));
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturerDto>> Create([FromBody] CreateManufacturerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.CompanyId == Guid.Empty)
        {
            return BadRequest(new { message = "CompanyId is required." });
        }

        var company = await _db.Companies.FindAsync(request.CompanyId);
        if (company is null)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _db.Branches.FindAsync(request.BranchId.Value);
            if (branch is null || branch.CompanyId != request.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to the specified company." });
            }
        }

        var manufacturer = new Manufacturer
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            Name = request.Name,
            Code = request.Code,
            ProductNameAtManufacturer = request.ProductNameAtManufacturer,
            Address = request.Address,
            Phone = request.Phone,
            Website = request.Website,
            ContactPerson = request.ContactPerson,
            CreatedAt = DateTime.UtcNow
        };

        _db.Manufacturers.Add(manufacturer);
        await _db.SaveChangesAsync();

        var dto = ToDto(manufacturer);
        return CreatedAtAction(nameof(GetById), new { id = manufacturer.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ManufacturerDto>> Update(Guid id, [FromBody] UpdateManufacturerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var manufacturer = await _db.Manufacturers.FirstOrDefaultAsync(m => m.Id == id);
        if (manufacturer is null)
        {
            return NotFound();
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _db.Branches.FindAsync(request.BranchId.Value);
            if (branch is null || branch.CompanyId != manufacturer.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to the manufacturer's company." });
            }
        }

        manufacturer.BranchId = request.BranchId;
        manufacturer.Name = request.Name;
        manufacturer.Code = request.Code;
        manufacturer.ProductNameAtManufacturer = request.ProductNameAtManufacturer;
        manufacturer.Address = request.Address;
        manufacturer.Phone = request.Phone;
        manufacturer.Website = request.Website;
        manufacturer.ContactPerson = request.ContactPerson;
        manufacturer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(manufacturer));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var manufacturer = await _db.Manufacturers.FirstOrDefaultAsync(m => m.Id == id);
        if (manufacturer is null)
        {
            return NotFound();
        }

        var hasProducts = await _db.Products.AnyAsync(p => p.ManufacturerId == id);
        if (hasProducts)
        {
            return BadRequest(new { message = "Cannot delete manufacturer with associated products." });
        }

        _db.Manufacturers.Remove(manufacturer);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly Expression<Func<Manufacturer, ManufacturerDto>> ToDtoExpression = entity => new ManufacturerDto
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        BranchId = entity.BranchId,
        Name = entity.Name,
        Code = entity.Code,
        ProductNameAtManufacturer = entity.ProductNameAtManufacturer,
        Address = entity.Address,
        Phone = entity.Phone,
        Website = entity.Website,
        ContactPerson = entity.ContactPerson
    };

    private static ManufacturerDto ToDto(Manufacturer entity) => ToDtoExpression.Compile().Invoke(entity);
}
