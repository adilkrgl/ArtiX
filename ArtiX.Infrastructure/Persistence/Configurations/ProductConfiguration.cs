using ArtiX.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProductType)
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
