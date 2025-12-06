using System.Data;
using System.Linq;
using System.Collections.Generic;
using ArtiX.Api.Dtos.Sales;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Infrastructure.Persistence;
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
    
    public InvoicesController(ErpDbContext db)
    {
        _db = db;
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
        var validation = await ValidateHeaderAsync(request.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);

        var yearPrefix = DateTime.UtcNow.Year.ToString();
        var maxForYear = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(yearPrefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        var nextSequence = 1;
        if (!string.IsNullOrWhiteSpace(maxForYear) && maxForYear.Length > 4)
        {
            var numericPart = maxForYear.Substring(4);
            if (int.TryParse(numericPart, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        var invoiceNumber = $"{yearPrefix}{nextSequence:D5}";

        var lineRequests = request.Lines ?? new List<CreateInvoiceLineItem>();

        var productIds = lineRequests
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(HttpContext.RequestAborted);

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            SalesChannelId = request.SalesChannelId,
            SalesRepresentativeId = request.SalesRepresentativeId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = request.InvoiceDate,
            CurrencyCode = request.CurrencyCode,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in lineRequests)
        {
            if (line.ProductId == null && string.IsNullOrWhiteSpace(line.CustomDescription))
            {
                return BadRequest(new { message = "Custom lines must include a description when ProductId is not provided." });
            }

            var product = line.ProductId.HasValue
                ? products.FirstOrDefault(p => p.Id == line.ProductId.Value)
                : null;

            var unitPrice = product?.RetailPrice ?? 0m;
            var taxRate = product?.TaxRate ?? 0m;
            var lineSubtotal = line.Quantity * unitPrice;
            var discountAmount = (line.DiscountRate / 100m) * lineSubtotal;
            var lineTotal = lineSubtotal - discountAmount;
            var taxAmount = (taxRate / 100m) * lineTotal;
            var lineTotalWithTax = lineTotal + taxAmount;

            entity.Lines.Add(new InvoiceLine
            {
                Id = Guid.NewGuid(),
                Invoice = entity,
                ProductId = product?.Id,
                Product = product,
                ProductSku = product?.Sku,
                ProductName = product?.Name,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                DiscountRate = line.DiscountRate,
                DiscountAmount = discountAmount,
                LineSubtotal = lineSubtotal,
                LineTotal = lineTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                LineTotalWithTax = lineTotalWithTax,
                CustomDescription = line.CustomDescription,
                LineNote = line.LineNote
            });
        }

        entity.Subtotal = entity.Lines.Sum(x => x.LineSubtotal);
        entity.DiscountTotal = entity.Lines.Sum(x => x.DiscountAmount);
        entity.TaxTotal = entity.Lines.Sum(x => x.TaxAmount);
        entity.Total = entity.Lines.Sum(x => x.LineTotalWithTax);

        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync(HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
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

        var validation = await ValidateHeaderAsync(invoice.CompanyId, request.BranchId, request.CustomerId, request.SalesChannelId, request.SalesRepresentativeId);
        if (validation is ObjectResult error)
        {
            return error;
        }

        invoice.BranchId = request.BranchId;
        invoice.CustomerId = request.CustomerId;
        invoice.SalesChannelId = request.SalesChannelId;
        invoice.SalesRepresentativeId = request.SalesRepresentativeId;
        invoice.InvoiceDate = request.InvoiceDate;
        invoice.CurrencyCode = request.CurrencyCode;
        invoice.ExchangeRate = request.ExchangeRate;
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

    private async Task<IActionResult?> ValidateHeaderAsync(Guid companyId, Guid? branchId, Guid? customerId, Guid? salesChannelId, Guid? salesRepresentativeId)
    {
        var companyExists = await _db.Companies.AnyAsync(x => x.Id == companyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        if (branchId.HasValue)
        {
            var branchValid = await _db.Branches.AnyAsync(x => x.Id == branchId.Value && x.CompanyId == companyId);
            if (!branchValid)
            {
                return BadRequest(new { message = "Branch not found for the given company." });
            }
        }

        if (customerId.HasValue)
        {
            var customerValid = await _db.Customers.AnyAsync(x => x.Id == customerId.Value && x.CompanyId == companyId);
            if (!customerValid)
            {
                return BadRequest(new { message = "Customer not found for the given company." });
            }
        }

        if (salesChannelId.HasValue)
        {
            var channelValid = await _db.SalesChannels.AnyAsync(x => x.Id == salesChannelId.Value && x.CompanyId == companyId);
            if (!channelValid)
            {
                return BadRequest(new { message = "Sales channel not found for the given company." });
            }
        }

        if (salesRepresentativeId.HasValue)
        {
            var repValid = await _db.SalesRepresentatives.AnyAsync(x => x.Id == salesRepresentativeId.Value && x.CompanyId == companyId);
            if (!repValid)
            {
                return BadRequest(new { message = "Sales representative not found for the given company." });
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
            BranchId = invoice.BranchId,
            CustomerId = invoice.CustomerId,
            SalesChannelId = invoice.SalesChannelId,
            SalesRepresentativeId = invoice.SalesRepresentativeId,
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
                InvoiceId = l.InvoiceId,
                ProductId = l.ProductId,
                ProductSku = l.ProductSku,
                ProductName = l.ProductName,
                CustomDescription = l.CustomDescription,
                LineNote = l.LineNote,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountRate = l.DiscountRate,
                DiscountAmount = l.DiscountAmount,
                LineSubtotal = l.LineSubtotal,
                LineTotal = l.LineTotal,
                TaxRate = l.TaxRate,
                TaxAmount = l.TaxAmount,
                LineTotalWithTax = l.LineTotalWithTax
            }).ToList()
        };
    }
}
