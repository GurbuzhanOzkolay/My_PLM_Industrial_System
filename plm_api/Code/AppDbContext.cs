using Microsoft.EntityFrameworkCore;

namespace plm_api.Code
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // TABLOLARIMIZ
        public DbSet<Products> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Kategori Kendi Kendine İlişki
            modelBuilder.Entity<Category>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Ürün ve Kategori Arasındaki Çoktan Çoğa İlişki
            modelBuilder.Entity<Products>()
                .HasMany(x => x.Categories)
                .WithMany(x => x.Products);

            // 3. ProductItem: Ana Ürün - Alt Bileşen İlişkisi
            modelBuilder.Entity<ProductItem>()
                .HasOne(pi => pi.ParentProduct)
                .WithMany()
                .HasForeignKey(pi => pi.ParentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductItem>()
                .HasOne(pi => pi.ChildProduct)
                .WithMany()
                .HasForeignKey(pi => pi.ChildProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 