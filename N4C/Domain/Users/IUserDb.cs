using Microsoft.EntityFrameworkCore;

namespace N4C.Domain.Users
{
    public interface IUserDb
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
    }
}
