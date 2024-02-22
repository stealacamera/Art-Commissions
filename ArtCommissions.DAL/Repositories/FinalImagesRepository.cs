using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IFinalImagesRepository : IStrongEntityRepository<FinalImage, int>
{
    Task<FinalImage?> GetByOrderAsync(int orderId);
}

internal class FinalImagesRepository : StrongEntityRepository<FinalImage, int>, IFinalImagesRepository
{
    public FinalImagesRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<FinalImage?> GetByOrderAsync(int orderId)
    {
        IQueryable<FinalImage> query = _untrackedSet;
        query = query.Where(e => e.OrderId == orderId);

        return await query.FirstOrDefaultAsync();
    }
}
