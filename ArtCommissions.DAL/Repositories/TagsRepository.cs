using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface ITagsRepository : IStrongEntityRepository<Tag, int>
{
    Task<PaginatedEnumerable<Tag>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<Tag>> GetAllAsync();
}

internal class TagsRepository : StrongEntityRepository<Tag, int>, ITagsRepository
{
    public TagsRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PaginatedEnumerable<Tag>> GetAllAsync(int page, int pageSize)
    {
        IQueryable<Tag> query = _untrackedSet;
        query = query.OrderByDescending(e => e.Id);

        return await PaginatedEnumerable<Tag>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _untrackedSet.ToListAsync();
    }
}
