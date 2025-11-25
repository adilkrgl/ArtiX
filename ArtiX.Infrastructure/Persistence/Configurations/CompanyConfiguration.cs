using ArtiX.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired();
        builder.Property(c => c.Name).IsRequired();

        builder.HasMany(c => c.Branches)
            .WithOne(b => b.Company)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.SalesChannels)
            .WithOne(sc => sc.Company)
            .HasForeignKey(sc => sc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.SalesRepresentatives)
            .WithOne(sr => sr.Company)
            .HasForeignKey(sr => sr.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
