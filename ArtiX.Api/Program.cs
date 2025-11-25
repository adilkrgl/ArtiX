using ArtiX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ErpDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "Hello, ArtiX!");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        if (db.Database.CanConnect())
        {
            Console.WriteLine("✅ ErpDbContext can connect to the database.");
        }
        else
        {
            Console.WriteLine("❌ ErpDbContext cannot connect to the database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error while testing database connection:");
        Console.WriteLine(ex.ToString());
    }
}

app.Run();
