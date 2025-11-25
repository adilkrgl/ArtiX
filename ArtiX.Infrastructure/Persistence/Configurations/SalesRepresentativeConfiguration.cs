using ArtiX.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class SalesRepresentativeConfiguration : IEntityTypeConfiguration<SalesRepresentative>
{
    public void Configure(EntityTypeBuilder<SalesRepresentative> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.FullName).IsRequired();

        builder.HasOne(sr => sr.Company)
            .WithMany(c => c.SalesRepresentatives)
            .HasForeignKey(sr => sr.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
