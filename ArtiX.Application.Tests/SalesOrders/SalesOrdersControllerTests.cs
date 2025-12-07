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

public class SalesOrdersControllerTests
{
    [Fact]
    public async Task CreateSalesOrder_Should_Create_Order_With_Lines()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);
        var controller = new SalesOrdersController(context);

        var request = new CreateSalesOrderRequest
        {
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CustomerId = seed.CustomerId,
            SalesChannelId = seed.SalesChannelId,
            SalesRepresentativeId = seed.SalesRepresentativeId,
            OrderDate = new DateTime(2024, 01, 15, 10, 30, 00, DateTimeKind.Utc),
            Status = "Draft",
            Lines = new List<CreateSalesOrderLineRequest>
            {
                new()
                {
                    SalesOrderId = Guid.Empty,
                    ProductId = seed.ProductId,
                    Quantity = 2,
                    UnitPrice = 50m,
                    CustomDescription = "Line desc",
                    LineNote = "Line note"
                }
            }
        };

        var result = await controller.CreateAsync(request);

        var created = result.Result as CreatedAtActionResult;
        created.Should().NotBeNull();

        var dto = created!.Value as SalesOrderDto;
        dto.Should().NotBeNull();
        dto!.Lines.Should().HaveCount(1);
        dto.TotalAmount.Should().Be(100m);

        var savedOrder = await context.SalesOrders.Include(x => x.Lines).SingleAsync(x => x.Id == dto.Id);
        savedOrder.Lines.Should().ContainSingle();
        savedOrder.Lines.Single().Quantity.Should().Be(2);
        savedOrder.Lines.Single().UnitPrice.Should().Be(50m);
    }

    [Fact]
    public async Task GetSalesOrders_Should_Filter_By_Company_And_Customer()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);
        var otherCompanyId = Guid.NewGuid();

        context.Companies.Add(new Company
        {
            Id = otherCompanyId,
            Name = "Other Co",
            Code = "OTH",
            CreatedAt = DateTime.UtcNow
        });

        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = otherCompanyId,
            Name = "Other Customer",
            Code = "OC",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        var order1 = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CustomerId = seed.CustomerId,
            OrderDate = new DateTime(2024, 02, 01, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        var order2 = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = otherCompanyId,
            OrderDate = new DateTime(2024, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        context.SalesOrders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var controller = new SalesOrdersController(context);

        var response = await controller.GetAsync(seed.CompanyId, seed.BranchId, seed.CustomerId);

        var okResult = response.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var items = okResult!.Value as List<SalesOrderDto>;
        items.Should().NotBeNull();
        items!.Should().HaveCount(1);
        items.Single().Id.Should().Be(order1.Id);
    }

    [Fact]
    public async Task UpdateSalesOrder_Should_Update_Status_And_Customer()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);

        var secondCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            Name = "Second Customer",
            Code = "CUST-2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(secondCustomer);

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CustomerId = seed.CustomerId,
            Status = "Draft",
            OrderDate = new DateTime(2024, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        context.SalesOrders.Add(order);
        await context.SaveChangesAsync();

        var controller = new SalesOrdersController(context);

        var updateRequest = new UpdateSalesOrderRequest
        {
            CustomerId = secondCustomer.Id,
            Status = "Confirmed"
        };

        var result = await controller.UpdateAsync(order.Id, updateRequest);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var dto = okResult!.Value as SalesOrderDto;
        dto.Should().NotBeNull();
        dto!.CustomerId.Should().Be(secondCustomer.Id);
        dto.Status.Should().Be("Confirmed");

        var updated = await context.SalesOrders.FindAsync(order.Id);
        updated!.CustomerId.Should().Be(secondCustomer.Id);
        updated.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task DeleteSalesOrder_Should_Remove_Order_And_Lines()
    {
        using var context = TestDbContextFactory.Create();
        var seed = await SeedReferenceDataAsync(context);

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = seed.CompanyId,
            BranchId = seed.BranchId,
            CustomerId = seed.CustomerId,
            OrderDate = new DateTime(2024, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            Lines = new List<SalesOrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = seed.ProductId,
                    Quantity = 1,
                    UnitPrice = 25m,
                    CompanyId = seed.CompanyId,
                    BranchId = seed.BranchId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        context.SalesOrders.Add(order);
        await context.SaveChangesAsync();

        var controller = new SalesOrdersController(context);

        var result = await controller.DeleteAsync(order.Id);

        result.Should().BeOfType<NoContentResult>();

        (await context.SalesOrders.AnyAsync(x => x.Id == order.Id)).Should().BeFalse();
        (await context.SalesOrderLines.AnyAsync(x => x.SalesOrderId == order.Id)).Should().BeFalse();
    }

    private static async Task<SeedData> SeedReferenceDataAsync(ArtiX.Infrastructure.Persistence.ErpDbContext context)
    {
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var salesChannelId = Guid.NewGuid();
        var salesRepresentativeId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var company = new Company
        {
            Id = companyId,
            Name = "Main Co",
            Code = "MAIN",
            CreatedAt = DateTime.UtcNow
        };

        var branch = new Branch
        {
            Id = branchId,
            CompanyId = companyId,
            Name = "HQ",
            Code = "HQ",
            BranchType = BranchType.Store,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Id = customerId,
            CompanyId = companyId,
            BranchId = branchId,
            Name = "Customer",
            Code = "CUST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var channel = new SalesChannel
        {
            Id = salesChannelId,
            CompanyId = companyId,
            Name = "Online",
            Code = "ONL",
            ChannelType = SalesChannelType.Website,
            IsOnline = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var representative = new SalesRepresentative
        {
            Id = salesRepresentativeId,
            CompanyId = companyId,
            FullName = "Rep Name",
            CreatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = productId,
            CompanyId = companyId,
            Name = "Product",
            CostPrice = 10m,
            RetailPrice = 20m,
            WholesalePrice = 15m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Companies.Add(company);
        context.Branches.Add(branch);
        context.Customers.Add(customer);
        context.SalesChannels.Add(channel);
        context.SalesRepresentatives.Add(representative);
        context.Products.Add(product);

        await context.SaveChangesAsync();

        return new SeedData(companyId, branchId, customerId, salesChannelId, salesRepresentativeId, productId);
    }

    private record SeedData(Guid CompanyId, Guid BranchId, Guid CustomerId, Guid SalesChannelId, Guid SalesRepresentativeId, Guid ProductId);
}
