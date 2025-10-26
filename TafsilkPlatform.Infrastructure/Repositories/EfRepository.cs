using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public EfRepository(AppDbContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        // Works for entities with Guid single primary key. For others, this returns null.
        return await _set.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _set.AsNoTracking().ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _set.Where(predicate).AsNoTracking().ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _set.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate is null ? await _set.CountAsync() : await _set.CountAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
    {
        var query = predicate is null ? _set.AsQueryable() : _set.Where(predicate);
        return await query.AsNoTracking().Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}
