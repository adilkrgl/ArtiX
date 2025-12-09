using ArtiX.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");

        builder.Property(o => o.CompanyId)
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasMaxLength(128);

        builder.HasOne(o => o.Company)
            .WithMany()
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Branch)
            .WithMany()
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SalesChannel)
            .WithMany()
            .HasForeignKey(o => o.SalesChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SalesRepresentative)
            .WithMany()
            .HasForeignKey(o => o.SalesRepresentativeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
