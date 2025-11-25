using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ArtiX.Infrastructure.Persistence;

#nullable disable

namespace ArtiX.Infrastructure.Migrations
{
    [DbContext(typeof(ErpDbContext))]
    partial class ErpDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");
        }
    }
}
