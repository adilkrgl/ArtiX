using ArtiX.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtiX.Infrastructure.Persistence.Configurations;

public class SalesChannelConfiguration : IEntityTypeConfiguration<SalesChannel>
{
    public void Configure(EntityTypeBuilder<SalesChannel> builder)
    {
        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Code).IsRequired();
        builder.Property(sc => sc.Name).IsRequired();

        builder.HasOne(sc => sc.Company)
            .WithMany(c => c.SalesChannels)
            .HasForeignKey(sc => sc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
