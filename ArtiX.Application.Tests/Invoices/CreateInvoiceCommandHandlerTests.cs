using ArtiX.Application.Invoices;
using ArtiX.Application.Invoices.Commands;
using ArtiX.Application.Tests.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Infrastructure.Invoices;
using ArtiX.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ArtiX.Application.Tests.Invoices;

public class CreateInvoiceCommandHandlerTests
{
    [Fact]
    public async Task CreateInvoice_Should_Calculate_Totals_Correctly()
    {
        var companyId = Guid.Parse("912c89b5-d145-4442-a857-08de2cfdb836");
        var customerId = Guid.Parse("2612d110-2bc5-4f90-901d-27038b4d0a19");
        var productId = Guid.Parse("5a4898b6-ee19-4f6a-942f-ee9c71328ad5");

        var (context, handler, _) = CreateHandlerWithSeededProduct(
            companyId,
            productId,
            600m,
            20m,
            "3x2 custom-made sofa",
            "SOFA-3X2",
            customerId);

        using (context)
        {
            var command = new CreateInvoiceCommand
            {
                CompanyId = companyId,
                CustomerId = customerId,
                InvoiceDate = new DateTime(2025, 12, 5, 16, 20, 0),
                CurrencyCode = "GBP",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 4m,
                        DiscountRate = 0m,
                        CustomDescription = "3x2 custom-made sofa",
                        LineNote = "Fabric: Letto 1002, colour: Indigo, detailing: gold"
                    }
                }
            };

            var invoiceId = await handler.Handle(command, CancellationToken.None);

            var invoice = await context.Invoices
                .Include(i => i.Lines)
                .SingleAsync(i => i.Id == invoiceId);

            var line = invoice.Lines.Single();

            line.Quantity.Should().Be(4m);
            line.UnitPrice.Should().Be(600m);
            line.LineSubtotal.Should().Be(2400m);
            line.DiscountAmount.Should().Be(0m);
            line.LineTotal.Should().Be(2400m);
            line.TaxRate.Should().Be(20m);
            line.TaxAmount.Should().Be(480m);
            line.LineTotalWithTax.Should().Be(2880m);

            invoice.Subtotal.Should().Be(2400m);
            invoice.DiscountTotal.Should().Be(0m);
            invoice.TaxTotal.Should().Be(480m);
            invoice.Total.Should().Be(2880m);
        }
    }

    [Fact]
    public async Task CreateInvoice_Should_Apply_Discounts_And_Tax()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var (context, handler, _) = CreateHandlerWithSeededProduct(
            companyId,
            productId,
            100m,
            20m,
            "Discounted product",
            "DISC-100");

        using (context)
        {
            var command = new CreateInvoiceCommand
            {
                CompanyId = companyId,
                InvoiceDate = new DateTime(2024, 1, 1),
                CurrencyCode = "GBP",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 2m,
                        DiscountRate = 10m,
                        CustomDescription = "Discounted",
                        LineNote = ""
                    }
                }
            };

            var invoiceId = await handler.Handle(command, CancellationToken.None);

            var invoice = await context.Invoices
                .Include(i => i.Lines)
                .SingleAsync(i => i.Id == invoiceId);

            var line = invoice.Lines.Single();

            line.LineSubtotal.Should().Be(200m);
            line.DiscountAmount.Should().Be(20m);
            line.LineTotal.Should().Be(180m);
            line.TaxAmount.Should().Be(36m);
            line.LineTotalWithTax.Should().Be(216m);

            invoice.Subtotal.Should().Be(200m);
            invoice.DiscountTotal.Should().Be(20m);
            invoice.TaxTotal.Should().Be(36m);
            invoice.Total.Should().Be(216m);
        }
    }

    [Fact]
    public async Task CreateInvoice_Should_Handle_TaxExempt_Product()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var (context, handler, _) = CreateHandlerWithSeededProduct(
            companyId,
            productId,
            50m,
            0m,
            "Tax free product",
            "NT-50");

        using (context)
        {
            var command = new CreateInvoiceCommand
            {
                CompanyId = companyId,
                InvoiceDate = new DateTime(2024, 2, 2),
                CurrencyCode = "GBP",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 3m,
                        DiscountRate = 0m,
                        CustomDescription = "Tax free",
                        LineNote = null
                    }
                }
            };

            var invoiceId = await handler.Handle(command, CancellationToken.None);

            var invoice = await context.Invoices
                .Include(i => i.Lines)
                .SingleAsync(i => i.Id == invoiceId);

            var line = invoice.Lines.Single();

            line.LineSubtotal.Should().Be(150m);
            line.LineTotal.Should().Be(150m);
            line.TaxAmount.Should().Be(0m);
            line.LineTotalWithTax.Should().Be(150m);

            invoice.Subtotal.Should().Be(150m);
            invoice.TaxTotal.Should().Be(0m);
            invoice.Total.Should().Be(150m);
        }
    }

    [Fact]
    public async Task CreateInvoice_Should_Respect_Product_Tax_Inclusive_Mode()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var (context, handler, _) = CreateHandlerWithSeededProduct(
            companyId,
            productId,
            120m,
            20m,
            "Tax inclusive product",
            "TI-120",
            isTaxInclusive: true);

        using (context)
        {
            var command = new CreateInvoiceCommand
            {
                CompanyId = companyId,
                InvoiceDate = new DateTime(2024, 5, 1),
                CurrencyCode = "GBP",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 2m,
                        DiscountRate = 0m,
                        CustomDescription = "Inclusive item"
                    }
                }
            };

            var invoiceId = await handler.Handle(command, CancellationToken.None);

            var invoice = await context.Invoices
                .Include(i => i.Lines)
                .SingleAsync(i => i.Id == invoiceId);

            var line = invoice.Lines.Single();

            line.IsTaxInclusive.Should().BeTrue();
            line.LineSubtotal.Should().Be(200m);
            line.LineTotal.Should().Be(200m);
            line.TaxAmount.Should().Be(40m);
            line.LineTotalWithTax.Should().Be(240m);

            invoice.Subtotal.Should().Be(200m);
            invoice.DiscountTotal.Should().Be(0m);
            invoice.TaxTotal.Should().Be(40m);
            invoice.Total.Should().Be(240m);
        }
    }

    [Fact]
    public async Task CreateInvoice_Should_Apply_Discount_Before_Tax_Split_When_Inclusive()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var (context, handler, _) = CreateHandlerWithSeededProduct(
            companyId,
            productId,
            120m,
            20m,
            "Tax inclusive discounted",
            "TI-DISC",
            isTaxInclusive: true);

        using (context)
        {
            var command = new CreateInvoiceCommand
            {
                CompanyId = companyId,
                InvoiceDate = new DateTime(2024, 6, 1),
                CurrencyCode = "GBP",
                Lines = new List<CreateInvoiceLineDto>
                {
                    new()
                    {
                        ProductId = productId,
                        Quantity = 2m,
                        DiscountRate = 10m,
                        CustomDescription = "Inclusive discounted"
                    }
                }
            };

            var invoiceId = await handler.Handle(command, CancellationToken.None);

            var invoice = await context.Invoices
                .Include(i => i.Lines)
                .SingleAsync(i => i.Id == invoiceId);

            var line = invoice.Lines.Single();

            line.IsTaxInclusive.Should().BeTrue();
            line.DiscountAmount.Should().Be(24m);
            line.LineSubtotal.Should().Be(180m);
            line.LineTotal.Should().Be(180m);
            line.TaxAmount.Should().Be(36m);
            line.LineTotalWithTax.Should().Be(216m);

            invoice.Subtotal.Should().Be(180m);
            invoice.DiscountTotal.Should().Be(24m);
            invoice.TaxTotal.Should().Be(36m);
            invoice.Total.Should().Be(216m);
        }
    }

    private static (ErpDbContext Context, CreateInvoiceCommandHandler Handler, Product Product) CreateHandlerWithSeededProduct(
        Guid companyId,
        Guid productId,
        decimal retailPrice,
        decimal taxRate,
        string productName,
        string sku,
        Guid? customerId = null,
        bool isTaxInclusive = false)
    {
        var context = TestDbContextFactory.Create();

        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            Code = "TEST",
            CreatedAt = DateTime.UtcNow
        };
        context.Companies.Add(company);

        if (customerId.HasValue)
        {
            var customer = new Customer
            {
                Id = customerId.Value,
                CompanyId = companyId,
                Company = company,
                Name = "Test Customer",
                Code = "CUST",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Customers.Add(customer);
        }

        var product = new Product
        {
            Id = productId,
            CompanyId = companyId,
            Company = company,
            Name = productName,
            Sku = sku,
            RetailPrice = retailPrice,
            IsTaxInclusive = isTaxInclusive,
            TaxRate = taxRate,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);

        context.SaveChanges();

        var invoiceNumberGenerator = new Mock<IInvoiceNumberGenerator>();
        invoiceNumberGenerator
            .Setup(x => x.GenerateNextAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TEST-00001");

        var handler = new CreateInvoiceCommandHandler(context, invoiceNumberGenerator.Object);

        return (context, handler, product);
    }
}
