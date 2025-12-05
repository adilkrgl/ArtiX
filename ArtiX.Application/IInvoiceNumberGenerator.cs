using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtiX.Application.Invoices;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateNextAsync(DateTime invoiceDate, CancellationToken cancellationToken = default);
}
