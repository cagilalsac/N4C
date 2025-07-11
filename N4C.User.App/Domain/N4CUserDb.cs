﻿using Microsoft.EntityFrameworkCore;

namespace N4C.User.App.Domain
{
    public class N4CUserDb : DbContext
    {
        public DbSet<N4CUser> Users { get; set; }
        public DbSet<N4CRole> Roles { get; set; }
        public DbSet<N4CUserRole> UserRoles { get; set; }
        public DbSet<N4CStatus> Statuses { get; set; }

        public N4CUserDb(DbContextOptions<N4CUserDb> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<N4CUserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<N4CUserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<N4CUser>().HasOne(u => u.Status).WithMany(s => s.Users).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
