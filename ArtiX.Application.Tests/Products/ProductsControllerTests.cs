using ArtiX.Api.Controllers.Products;
using ArtiX.Api.Dtos.Products; 
using ArtiX.Application.Tests.Common;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Products;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace ArtiX.Application.Tests.Products;

public class ProductsControllerTests
{
    [Fact]
    public async Task CreateProduct_WithManufacturerAndPrices_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var companyId = Guid.NewGuid();
        var manufacturerId = Guid.NewGuid();

        context.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme",
            Code = "ACM",
            CreatedAt = DateTime.UtcNow
        });

        context.Manufacturers.Add(new Manufacturer
        {
            Id = manufacturerId,
            CompanyId = companyId,
            Name = "Maker",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var controller = new ProductsController(context);

        var request = new CreateProductRequest
        {
            CompanyId = companyId,
            Name = "Widget",
            ManufacturerId = manufacturerId,
            CostPrice = 10m,
            RetailPrice = 15m,
            WholesalePrice = 12m,
            IsActive = true
        };

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result); // Declare 'created' before using it
        var dto = Assert.IsType<Api.Dtos.Products.ProductDto>(created.Value); // Use 'created' after declaration

        dto.Name.Should().Be("Widget");
        dto.ManufacturerId.Should().Be(manufacturerId);
        dto.CostPrice.Should().Be(10m);
        dto.RetailPrice.Should().Be(15m);
        dto.WholesalePrice.Should().Be(12m);
        dto.ManufacturerName.Should().Be("Maker");
    }

    [Fact]
    public async Task UpdateProduct_Prices_Should_Persist()
    {
        using var context = TestDbContextFactory.Create();
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        context.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme",
            Code = "ACM",
            CreatedAt = DateTime.UtcNow
        });

        context.Products.Add(new Product
        {
            Id = productId,
            CompanyId = companyId,
            Name = "Widget",
            CostPrice = 5m,
            RetailPrice = 8m,
            WholesalePrice = 6m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var controller = new ProductsController(context);

        var request = new UpdateProductRequest
        {
            Name = "Widget",
            CostPrice = 6m,
            RetailPrice = 9m,
            WholesalePrice = 7m,
            IsActive = true
        };

        var response = await controller.Update(productId, request);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var dto = Assert.IsType<Api.Dtos.Products.ProductDto>(okResult.Value);

        dto.CostPrice.Should().Be(6m);
        dto.RetailPrice.Should().Be(9m);
        dto.WholesalePrice.Should().Be(7m);

        var updated = await context.Products.FindAsync(productId);
        updated!.CostPrice.Should().Be(6m);
        updated.RetailPrice.Should().Be(9m);
        updated.WholesalePrice.Should().Be(7m);
    }

    [Fact]
    public async Task Manufacturer_Crud_Should_Work()
    {
        using var context = TestDbContextFactory.Create();
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        context.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme",
            Code = "ACM",
            CreatedAt = DateTime.UtcNow
        });

        context.Branches.Add(new Branch
        {
            Id = branchId,
            CompanyId = companyId,
            Name = "Main",
            Code = "MAIN",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var controller = new ManufacturersController(context);

        // Update the namespace of the `CreateManufacturerRequest` to match the expected type
        var createRequest = new ArtiX.Api.Dtos.Products.CreateManufacturerRequest
        {
            CompanyId = companyId,
            BranchId = branchId,
            Name = "Maker",
            Code = "MK",
            Address = "123 Street",
            Phone = "123",
            Website = "https://maker.test"
        };

        var createResult = await controller.Create(createRequest);
        var created = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdDto = Assert.IsType<ManufacturerDto>(created.Value);

        // Update the type of `updateRequest` to match the expected type
        var updateRequest = new ArtiX.Api.Dtos.Products.UpdateManufacturerRequest
        {
            BranchId = branchId,
            Name = "Maker Updated",
            Code = "MK2",
            Phone = "456"
        };

        var updateResult = await controller.Update(createdDto.Id, updateRequest);
        var updatedOk = Assert.IsType<OkObjectResult>(updateResult.Result);
        var updatedDto = Assert.IsType<ManufacturerDto>(updatedOk.Value);
        updatedDto.Name.Should().Be("Maker Updated");
        updatedDto.Code.Should().Be("MK2");
        updatedDto.Phone.Should().Be("456");

        var deleteResult = await controller.Delete(createdDto.Id);
        deleteResult.Should().BeOfType<NoContentResult>();
        (await context.Manufacturers.AnyAsync(m => m.Id == createdDto.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteManufacturer_WithProducts_Should_Return_BadRequest()
    {
        using var context = TestDbContextFactory.Create();
        var companyId = Guid.NewGuid();
        var manufacturerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        context.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme",
            Code = "ACM",
            CreatedAt = DateTime.UtcNow
        });

        context.Manufacturers.Add(new Manufacturer
        {
            Id = manufacturerId,
            CompanyId = companyId,
            Name = "Maker",
            CreatedAt = DateTime.UtcNow
        });

        context.Products.Add(new Product
        {
            Id = productId,
            CompanyId = companyId,
            Name = "Widget",
            ManufacturerId = manufacturerId,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var controller = new ManufacturersController(context);

        var result = await controller.Delete(manufacturerId);

        result.Should().BeOfType<BadRequestObjectResult>();
        (await context.Manufacturers.AnyAsync(m => m.Id == manufacturerId)).Should().BeTrue();
    }
}
