using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity> AddAsync(TEntity model);
    void Remove(TEntity model);
}

internal abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly DbSet<TEntity> _set;
    protected readonly IQueryable<TEntity> _untrackedSet;

    public BaseRepository(AppDbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
        _untrackedSet = _set.AsNoTracking();
    }

    public async Task<TEntity> AddAsync(TEntity model)
    {
        await _set.AddAsync(model);
        return model;
    }

    public void Remove(TEntity model)
    {
        _set.Remove(model);
    }
}
