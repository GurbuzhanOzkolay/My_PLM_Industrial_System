using Microsoft.EntityFrameworkCore;

namespace plm_api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // TABLES
        public DbSet<Products> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        // RELATIONS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  1. Kategori Kendi Kendine İlişki 
            modelBuilder.Entity<Category>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            //  2. Ürün ve Kategori Arasındaki İlişki 
            modelBuilder.Entity<Products>()
                .HasOne(x => x.Category)         // Her ürünün bir kategorisi olur
                .WithMany(x => x.Products)       // Her kategorinin birden fazla ürünü olabilir
                .HasForeignKey(x => x.CategoryId)// İlişkiyi kuracak olan kolon CategoryId'dir
                .OnDelete(DeleteBehavior.SetNull); // Kategori silinirse ürünlerin kategorisi null olsun, ürünler silinmesin
        }
    }
}