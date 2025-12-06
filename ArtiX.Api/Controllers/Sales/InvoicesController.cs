using System.Linq;
using System.Collections.Generic;
using ArtiX.Api.Dtos.Sales;
using ArtiX.Application.Invoices;
using ArtiX.Application.Invoices.Commands;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InvoicesController : ControllerBase
{
    private readonly ErpDbContext _db;
    private readonly IMediator _mediator;

    public InvoicesController(ErpDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetAsync([FromQuery] Guid? companyId, [FromQuery] Guid? branchId, [FromQuery] Guid? customerId)
    {
        var query = _db.Invoices
            .Include(x => x.Lines)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId.Value);
        }

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        var items = await query.AsNoTracking().ToListAsync();
        return items.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        return ToDto(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateAsync([FromBody] CreateInvoiceRequest request)
    {
        var validation = await ValidateHeaderAsync(request.CompanyId, request.CustomerId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        var command = new CreateInvoiceCommand
        {
            CompanyId = request.CompanyId,
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate,
            CurrencyCode = request.CurrencyCode,
            Lines = (request.Lines ?? new List<CreateInvoiceLineRequest>()).Select(l => new CreateInvoiceLineDto
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                DiscountRate = l.DiscountRate,
                CustomDescription = l.CustomDescription,
                LineNote = l.LineNote
            }).ToList()
        };

        var invoiceId = await _mediator.Send(command, HttpContext.RequestAborted);

        var createdInvoice = await _db.Invoices
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoiceId, HttpContext.RequestAborted);

        if (createdInvoice == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetById), new { id = createdInvoice.Id }, ToDto(createdInvoice));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> UpdateAsync(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        var validation = await ValidateHeaderAsync(invoice.CompanyId, request.CustomerId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        invoice.CustomerId = request.CustomerId;
        invoice.InvoiceDate = request.InvoiceDate;
        invoice.CurrencyCode = request.CurrencyCode;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToDto(invoice);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        if (invoice.Lines.Any())
        {
            return BadRequest(new { message = "Cannot delete invoice with existing lines." });
        }

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateHeaderAsync(Guid companyId, Guid? customerId)
    {
        var companyExists = await _db.Companies.AnyAsync(x => x.Id == companyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (customerId.HasValue)
        {
            var customerValid = await _db.Customers.AnyAsync(x => x.Id == customerId.Value && x.CompanyId == companyId);
            if (!customerValid)
            {
                return BadRequest(new { message = "Customer not found for the given company." });
            }
        }

        return null;
    }

    private static InvoiceDto ToDto(Invoice invoice)
    {
        var lines = invoice.Lines ?? Enumerable.Empty<InvoiceLine>();
        return new InvoiceDto
        {
            Id = invoice.Id,
            CompanyId = invoice.CompanyId,
            CustomerId = invoice.CustomerId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = invoice.ExchangeRate,
            Subtotal = invoice.Subtotal,
            DiscountTotal = invoice.DiscountTotal,
            TaxTotal = invoice.TaxTotal,
            Total = invoice.Total,
            Lines = lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductSku = l.ProductSku,
                ProductName = l.ProductName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountRate = l.DiscountRate,
                DiscountAmount = l.DiscountAmount,
                LineSubtotal = l.LineSubtotal,
                LineTotal = l.LineTotal,
                TaxRate = l.TaxRate,
                TaxAmount = l.TaxAmount,
                LineTotalWithTax = l.LineTotalWithTax,
                CustomDescription = l.CustomDescription,
                LineNote = l.LineNote
            }).ToList()
        };
    }
}
