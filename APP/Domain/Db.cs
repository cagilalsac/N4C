using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using N4C.Domain;

namespace APP.Domain
{
    public class Db : DbContext, IDb
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductStore> ProductStores { get; set; }

        public Db(DbContextOptions<Db> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasOne(p => p._Category).WithMany(c => c._Products).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProductStore>().HasOne(ps => ps._Product).WithMany(p => p._ProductStores).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ProductStore>().HasOne(ps => ps._Store).WithMany(s => s._ProductStores).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class DbFactory : IDesignTimeDbContextFactory<Db>
    {
        public Db CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Db>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=N4C;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new Db(optionsBuilder.Options);
        }
    }
}
