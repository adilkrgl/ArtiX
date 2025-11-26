using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Customers;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CustomersController : ControllerBase
{
    private readonly ErpDbContext _context;

    public CustomersController(ErpDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> Get(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? salesRepresentativeId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        if (salesRepresentativeId.HasValue)
        {
            query = query.Where(x => x.DefaultSalesRepresentativeId == salesRepresentativeId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => EF.Property<bool?>(x, "IsActive") == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x => (EF.Property<string?>(x, "Name") ?? string.Empty).ToLower().Contains(term)
                                     || (EF.Property<string?>(x, "Code") ?? string.Empty).ToLower().Contains(term)
                                     || (EF.Property<string?>(x, "TaxNumber") ?? string.Empty).ToLower().Contains(term));
        }

        var customers = await query.AsNoTracking().ToListAsync();
        return customers.Select(ToDto).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> Get(Guid id)
    {
        var entity = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        return ToDto(entity);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Post(CreateCustomerRequest request)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == request.CompanyId);
        if (company == null)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == request.BranchId.Value);
            if (branch == null || branch.CompanyId != request.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to company." });
            }
        }

        if (request.DefaultSalesRepresentativeId.HasValue)
        {
            var rep = await _context.SalesRepresentatives.FirstOrDefaultAsync(x => x.Id == request.DefaultSalesRepresentativeId.Value);
            if (rep == null || rep.CompanyId != request.CompanyId)
            {
                return BadRequest(new { message = "Sales representative not found or does not belong to company." });
            }
        }

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            DefaultSalesRepresentativeId = request.DefaultSalesRepresentativeId,
            CreatedAt = DateTime.UtcNow
        };

        SetString(entity, "Name", request.Name);
        SetString(entity, "Code", request.Code);
        SetString(entity, "TaxNumber", request.TaxNumber);
        SetBool(entity, "IsActive", request.IsActive);

        await _context.Customers.AddAsync(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Put(Guid id, UpdateCustomerRequest request)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == request.BranchId.Value);
            if (branch == null || branch.CompanyId != entity.CompanyId)
            {
                return BadRequest(new { message = "Branch not found or does not belong to company." });
            }
        }

        if (request.DefaultSalesRepresentativeId.HasValue)
        {
            var rep = await _context.SalesRepresentatives.FirstOrDefaultAsync(x => x.Id == request.DefaultSalesRepresentativeId.Value);
            if (rep == null || rep.CompanyId != entity.CompanyId)
            {
                return BadRequest(new { message = "Sales representative not found or does not belong to company." });
            }
        }

        entity.BranchId = request.BranchId;
        entity.DefaultSalesRepresentativeId = request.DefaultSalesRepresentativeId;
        SetString(entity, "Name", request.Name);
        SetString(entity, "Code", request.Code);
        SetString(entity, "TaxNumber", request.TaxNumber);
        SetBool(entity, "IsActive", request.IsActive);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{customerId}/contacts")]
    public async Task<ActionResult<List<CustomerContactDto>>> GetContacts(Guid customerId)
    {
        var exists = await _context.Customers.AsNoTracking().AnyAsync(x => x.Id == customerId);
        if (!exists)
        {
            return NotFound();
        }

        var contacts = await _context.CustomerContacts.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();

        return contacts.Select(ToDto).ToList();
    }

    [HttpGet("{customerId}/contacts/{contactId}")]
    public async Task<ActionResult<CustomerContactDto>> GetContact(Guid customerId, Guid contactId)
    {
        var contact = await _context.CustomerContacts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == contactId && x.CustomerId == customerId);

        if (contact == null)
        {
            return NotFound();
        }

        return ToDto(contact);
    }

    [HttpPost("{customerId}/contacts")]
    public async Task<ActionResult<CustomerContactDto>> PostContact(Guid customerId, CreateCustomerContactRequest request)
    {
        var customerExists = await _context.Customers.AsNoTracking().AnyAsync(x => x.Id == customerId);
        if (!customerExists)
        {
            return NotFound();
        }

        var entity = new CustomerContact
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _context.CustomerContacts.AddAsync(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContact), new { customerId, contactId = entity.Id }, ToDto(entity));
    }

    [HttpPut("{customerId}/contacts/{contactId}")]
    public async Task<ActionResult<CustomerContactDto>> PutContact(Guid customerId, Guid contactId, UpdateCustomerContactRequest request)
    {
        var entity = await _context.CustomerContacts.FirstOrDefaultAsync(x => x.Id == contactId && x.CustomerId == customerId);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(entity);
    }

    [HttpDelete("{customerId}/contacts/{contactId}")]
    public async Task<IActionResult> DeleteContact(Guid customerId, Guid contactId)
    {
        var entity = await _context.CustomerContacts.FirstOrDefaultAsync(x => x.Id == contactId && x.CustomerId == customerId);
        if (entity == null)
        {
            return NotFound();
        }

        _context.CustomerContacts.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static CustomerDto ToDto(Customer entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        BranchId = entity.BranchId,
        DefaultSalesRepresentativeId = entity.DefaultSalesRepresentativeId,
        Name = GetString(entity, "Name") ?? string.Empty,
        Code = GetString(entity, "Code"),
        TaxNumber = GetString(entity, "TaxNumber"),
        IsActive = GetBool(entity, "IsActive")
    };

    private static CustomerContactDto ToDto(CustomerContact entity) => new()
    {
        Id = entity.Id,
        CustomerId = entity.CustomerId,
        Name = entity.Name,
        Email = entity.Email,
        Phone = entity.Phone
    };

    private static string? GetString(Customer entity, string property)
    {
        var prop = entity.GetType().GetProperty(property);
        return prop?.GetValue(entity) as string;
    }

    private static bool GetBool(Customer entity, string property)
    {
        var prop = entity.GetType().GetProperty(property);
        if (prop?.GetValue(entity) is bool value)
        {
            return value;
        }

        return false;
    }

    private static void SetString(Customer entity, string property, string? value)
    {
        var prop = entity.GetType().GetProperty(property);
        if (prop?.CanWrite == true)
        {
            prop.SetValue(entity, value);
        }
    }

    private static void SetBool(Customer entity, string property, bool value)
    {
        var prop = entity.GetType().GetProperty(property);
        if (prop?.CanWrite == true)
        {
            prop.SetValue(entity, value);
        }
    }
}
