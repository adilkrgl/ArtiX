using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArtiX.Application.Invoices;
using ArtiX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Infrastructure.Invoices;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ErpDbContext _dbContext;

    public InvoiceNumberGenerator(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateNextAsync(DateTime invoiceDate, CancellationToken cancellationToken = default)
    {
        var prefix = invoiceDate.ToString("yyyy", CultureInfo.InvariantCulture);

        var maxExisting = await _dbContext.Invoices
            .Where(i => i.InvoiceNumber != null && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (!string.IsNullOrWhiteSpace(maxExisting) && maxExisting!.Length > 4)
        {
            var suffix = maxExisting[4..];
            if (int.TryParse(suffix, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{prefix}{nextSequence:D5}";
    }
}
