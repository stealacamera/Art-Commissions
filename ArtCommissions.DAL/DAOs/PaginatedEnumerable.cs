using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.DAOs;

public class PaginatedEnumerable<T> where T : class
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }

    public static async Task<PaginatedEnumerable<T>> CreatePaginatedResponseAsync(IQueryable<T> query, int page, int pageSize)
    {
        var count = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedEnumerable<T> { Items = items, TotalCount = count };
    }
}
