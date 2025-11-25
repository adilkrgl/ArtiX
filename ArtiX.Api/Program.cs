using ArtiX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "Hello, ArtiX!");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ErpDbContext>();
        var connection = db.Database.GetDbConnection();

        Console.WriteLine("=== ArtiX DB Startup Check ===");
        Console.WriteLine($"Provider: {connection.GetType().FullName}");
        Console.WriteLine($"Connection string: {connection.ConnectionString}");

        Console.WriteLine("Applying migrations...");
        db.Database.Migrate();

        var canConnect = db.Database.CanConnect();
        Console.WriteLine($"CanConnect: {canConnect}");
        Console.WriteLine("=== ArtiX DB Startup Check END ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error during database startup check:");
        Console.WriteLine(ex.ToString());
    }
}

app.Run();
