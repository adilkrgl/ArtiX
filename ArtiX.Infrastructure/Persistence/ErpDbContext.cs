using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Crm;
using ArtiX.Domain.Entities.Customers;
using ArtiX.Domain.Entities.Finance;
using ArtiX.Domain.Entities.Identity;
using ArtiX.Domain.Entities.Integrations;
using ArtiX.Domain.Entities.Inventory;
using ArtiX.Domain.Entities.Products;
using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    public DbSet<UserBranch> UserBranches => Set<UserBranch>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();

    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();

    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();

    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryAttributeValue> InventoryAttributeValues => Set<InventoryAttributeValue>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    public DbSet<Quotation> Quotations => Set<Quotation>();

    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();

    public DbSet<DeliveryNoteLine> DeliveryNoteLines => Set<DeliveryNoteLine>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();

    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);

        modelBuilder.Entity<StockMovement>(builder =>
        {
            builder.HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sm => sm.Warehouse)
                .WithMany()
                .HasForeignKey(sm => sm.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<InventoryItem>(builder =>
        {
            builder.HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ii => ii.Warehouse)
                .WithMany()
                .HasForeignKey(ii => ii.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(entityType => entityType.GetForeignKeys())
                     .Where(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Warehouse)))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }

        base.OnModelCreating(modelBuilder);
    }
}
