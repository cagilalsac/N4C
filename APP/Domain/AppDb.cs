using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using N4C.Domain;

namespace APP.Domain
{
    public interface IAppDb : IDb
    {
    }

    public class AppDb : DbContext, IAppDb
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductStore> ProductStores { get; set; }

        public AppDb(DbContextOptions<AppDb> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProductStore>().HasOne(ps => ps.Product).WithMany(p => p.ProductStores).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProductStore>().HasOne(ps => ps.Store).WithMany(s => s.ProductStores).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class AppDbFactory : IDesignTimeDbContextFactory<AppDb>
    {
        public AppDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=N4C;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new AppDb(optionsBuilder.Options);
        }
    }
}
