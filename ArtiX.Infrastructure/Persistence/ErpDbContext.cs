using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Infrastructure.Persistence;

public class ErpDbContext : DbContext
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();

    public DbSet<SalesRepresentative> SalesRepresentatives => Set<SalesRepresentative>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    public DbSet<Quotation> Quotations => Set<Quotation>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
