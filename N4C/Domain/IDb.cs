using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace N4C.Domain
{
    public interface IDb : IDisposable
    {
        public DbSet<TEntity> Set<TEntity>() where TEntity : class;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public ChangeTracker ChangeTracker { get; }
        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
