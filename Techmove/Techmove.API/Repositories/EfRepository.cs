using Microsoft.EntityFrameworkCore;
using Techmove.Data;

namespace Techmove.API.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> Query()
    {
        return _dbSet;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
