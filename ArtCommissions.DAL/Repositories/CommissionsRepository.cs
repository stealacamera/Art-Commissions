using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;

namespace ArtCommissions.DAL.Repositories;

public interface ICommissionsRepository : IStrongEntityRepository<Commission, int>
{
    Task<PaginatedEnumerable<Commission>> FilterByKeywords(int page, int pageSize, string? keyword = null, int? tagId = null, bool orderDescending = true);
    Task<PaginatedEnumerable<Commission>> GetAllAsync(int page, int pageSize, bool excludeClosed = true, bool orderDescending = true);
    Task<PaginatedEnumerable<Commission>> GetAllByUserAsync(int ownerId, int page, int pageSize, bool excludeClosed = false, bool orderDescending = true);
}

internal class CommissionsRepository : StrongEntityRepository<Commission, int>, ICommissionsRepository
{
    public CommissionsRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PaginatedEnumerable<Commission>> FilterByKeywords(int page, int pageSize, string? keyword = null, int? tagId = null, bool orderDescending = true)
    {
        IQueryable<Commission> query = _untrackedSet;

        query = query.Where(e => e.Owner.LockoutEnd != DateTime.MaxValue);
        query = query.Where(e => e.IsClosed == false);

        if (orderDescending)
            query = query.OrderByDescending(e => e.Id);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(e => e.Title.Contains(keyword));

        if (tagId != null && tagId > 0)
            query = query.Where(e => e.Tags.Any(e => e.TagId == tagId));

        return await PaginatedEnumerable<Commission>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<PaginatedEnumerable<Commission>> GetAllAsync(int page, int pageSize, bool excludeClosed = true, bool orderDescending = true)
    {
        IQueryable<Commission> query = _untrackedSet;
        query = query.Where(e => e.Owner.LockoutEnd != DateTime.MaxValue);

        if (orderDescending)
            query = query.OrderByDescending(e => e.Id);

        if (excludeClosed)
            query = query.Where(e => e.IsClosed == false);

        return await PaginatedEnumerable<Commission>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<PaginatedEnumerable<Commission>> GetAllByUserAsync(int ownerId, int page, int pageSize, bool excludeClosed = false, bool orderDescending = true)
    {
        IQueryable<Commission> query = _untrackedSet;
        query = query.Where(e => e.OwnerId == ownerId);

        if (orderDescending)
            query = query.OrderByDescending(x => x.Id);

        if (excludeClosed)
            query = query.Where(e => e.IsClosed == false);

        return await PaginatedEnumerable<Commission>.CreatePaginatedResponseAsync(query, page, pageSize);
    }
}
