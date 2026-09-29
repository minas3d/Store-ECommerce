using ECommerce521.DataAccess.EntityTypeConfigurations;
using Microsoft.EntityFrameworkCore;

namespace ECommerce521.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSubImage> productSubImages { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ECommerce521;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ProductColor>()
            //    .HasKey(e => new { e.ProductId, e.Color });

            //modelBuilder.Entity<ProductSubImage>()
            //    .HasKey(e => new { e.ProductId, e.Img });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductSubImageEntityTypeConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
