using ArtiX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Application.Tests.Common;

public static class TestDbContextFactory
{
    public static ErpDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ErpDbContext(options);

        return context;
    }

    public static void Destroy(ErpDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
