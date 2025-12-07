using ArtiX.Api.Controllers.Sales;
using ArtiX.Api.Dtos.Sales;
using ArtiX.Application.Tests.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using ArtiX.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Application.Tests.SalesOrders;

public class SalesOrderLinesControllerTests
{
    [Fact]
    public async Task CreateSalesOrderLine_Should_Fail_If_Order_Not_Found()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new SalesOrderLinesController(context);

        var request = new CreateSalesOrderLineRequest
        {
            SalesOrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 1,
            UnitPrice = 10m
        };

        var result = await controller.CreateAsync(request);

        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();

        badRequest!.Value.Should().BeEquivalentTo(new { message = "SalesOrder not found" });
    }

    [Fact]
    public async Task CreateSalesOrderLine_Should_Create_Line()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);
        var controller = new SalesOrderLinesController(context);

        var request = new CreateSalesOrderLineRequest
        {
            SalesOrderId = seed.SalesOrderId,
            ProductId = seed.ProductId,
            Quantity = 3,
            UnitPrice = 12m,
            CustomDescription = "Custom",
            LineNote = "Note"
        };

        var result = await controller.CreateAsync(request);

        var created = result.Result as CreatedAtActionResult;
        created.Should().NotBeNull();

        var dto = created!.Value as SalesOrderLineDto;
        dto.Should().NotBeNull();
        dto!.SalesOrderId.Should().Be(seed.SalesOrderId);
        dto.Quantity.Should().Be(3);
        dto.UnitPrice.Should().Be(12m);
        dto.CustomDescription.Should().Be("Custom");
        dto.LineNote.Should().Be("Note");

        var saved = await context.SalesOrderLines.FindAsync(dto.Id);
        saved.Should().NotBeNull();
        saved!.SalesOrderId.Should().Be(seed.SalesOrderId);
        saved.Quantity.Should().Be(3);
        saved.UnitPrice.Should().Be(12m);
    }

    [Fact]
    public async Task UpdateSalesOrderLine_Should_Change_Quantity_And_Price()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);

        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = seed.SalesOrderId,
            ProductId = seed.ProductId,
            Quantity = 1,
            UnitPrice = 5m,
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CreatedAt = DateTime.UtcNow
        };

        context.SalesOrderLines.Add(line);
        await context.SaveChangesAsync();

        var controller = new SalesOrderLinesController(context);

        var request = new UpdateSalesOrderLineRequest
        {
            Quantity = 10,
            UnitPrice = 7.5m,
            CustomDescription = "Updated",
            LineNote = "Updated note"
        };

        var result = await controller.UpdateAsync(line.Id, request);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var dto = okResult!.Value as SalesOrderLineDto;
        dto.Should().NotBeNull();
        dto!.Quantity.Should().Be(10);
        dto.UnitPrice.Should().Be(7.5m);

        var updated = await context.SalesOrderLines.FindAsync(line.Id);
        updated!.Quantity.Should().Be(10);
        updated.UnitPrice.Should().Be(7.5m);
    }

    [Fact]
    public async Task DeleteSalesOrderLine_Should_Remove_Line()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);

        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = seed.SalesOrderId,
            ProductId = seed.ProductId,
            Quantity = 2,
            UnitPrice = 8m,
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CreatedAt = DateTime.UtcNow
        };

        context.SalesOrderLines.Add(line);
        await context.SaveChangesAsync();

        var controller = new SalesOrderLinesController(context);

        var result = await controller.DeleteAsync(line.Id);

        result.Should().BeOfType<NoContentResult>();
        (await context.SalesOrderLines.AnyAsync(x => x.Id == line.Id)).Should().BeFalse();
    }

    private static async Task<SeedData> SeedReferenceDataAsync(ArtiX.Infrastructure.Persistence.ErpDbContext context)
    {
        var seed = await SalesOrdersControllerTestsSeed.SeedReferenceDataAsync(context);

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CustomerId = seed.CustomerId,
            OrderDate = new DateTime(2024, 05, 01, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        context.SalesOrders.Add(order);
        await context.SaveChangesAsync();

        return new SeedData(seed.CompanyId, seed.BranchId, seed.CustomerId, seed.ProductId, order.Id);
    }

    private record SeedData(Guid CompanyId, Guid BranchId, Guid CustomerId, Guid ProductId, Guid SalesOrderId);

    private static class SalesOrdersControllerTestsSeed
    {
        public static async Task<SeedHelperData> SeedReferenceDataAsync(ArtiX.Infrastructure.Persistence.ErpDbContext context)
        {
            var companyId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var company = new Company
            {
                Id = companyId,
                Name = "Seed Co",
                Code = "SEED",
                CreatedAt = DateTime.UtcNow
            };

            var branch = new Branch
            {
                Id = branchId,
                CompanyId = companyId,
                Name = "Seed Branch",
                Code = "SEED-B",
                BranchType = BranchType.Store,
                CreatedAt = DateTime.UtcNow
            };

            var customer = new Customer
            {
                Id = customerId,
                CompanyId = companyId,
                BranchId = branchId,
                Name = "Seed Customer",
                Code = "SEED-C",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var product = new Product
            {
                Id = productId,
                CompanyId = companyId,
                Name = "Seed Product",
                CostPrice = 5m,
                RetailPrice = 10m,
                WholesalePrice = 8m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);
            context.Branches.Add(branch);
            context.Customers.Add(customer);
            context.Products.Add(product);

            await context.SaveChangesAsync();

            return new SeedHelperData(companyId, branchId, customerId, productId);
        }
    }

    private record SeedHelperData(Guid CompanyId, Guid BranchId, Guid CustomerId, Guid ProductId);
}
