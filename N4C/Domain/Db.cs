using Microsoft.EntityFrameworkCore;
using N4C.Domain.Users;

namespace N4C.Domain
{
    public class Db : DbContext, IDb
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public Db(DbContextOptions<Db> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
