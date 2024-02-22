using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface ICommissionSampleImagesRepository : IStrongEntityRepository<CommissionSampleImage, int>
{
    Task<IEnumerable<CommissionSampleImage>> GetAllByCommissionAsync(int commissionId);
    Task RemoveAllByCommissionAsync(int commissionId);
}

internal class CommissionSampleImagesRepository : StrongEntityRepository<CommissionSampleImage, int>, ICommissionSampleImagesRepository
{
    public CommissionSampleImagesRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<CommissionSampleImage>> GetAllByCommissionAsync(int commissionId)
    {
        IQueryable<CommissionSampleImage> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);
        
        return await query.ToListAsync();
    }

    public async Task RemoveAllByCommissionAsync(int commissionId)
    {
        IQueryable<CommissionSampleImage> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);
        
        await query.ExecuteDeleteAsync();
    }
}
