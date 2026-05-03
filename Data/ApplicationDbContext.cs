using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myhijyenProje.Models;

namespace myhijyenProje.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // decimal truncation uyarısı gelmesin
            builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            builder.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);
            builder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);
        }
    }
}