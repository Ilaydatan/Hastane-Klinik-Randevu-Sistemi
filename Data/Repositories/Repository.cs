using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Data.Repositories
{
    /// <summary>
    /// Generic Repository implementasyonu — IRepository'yi uygular.
    /// OOP: Interface implementasyonu, Generic sınıf örneği.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> TumunuGetirAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> IdIleGetirAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> EkleAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task GuncelleAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task SilAsync(int id)
        {
            var entity = await IdIleGetirAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual async Task<bool> VarMiAsync(int id)
        {
            var entity = await IdIleGetirAsync(id);
            return entity != null;
        }

        public virtual async Task<int> KaydetAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
