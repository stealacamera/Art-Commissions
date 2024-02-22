using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface ICommissionTagsRepository : IWeakEntityRepository<CommissionTag, int, int>
{
    Task<IEnumerable<CommissionTag>> GetAllForCommissionAsync(int commissionId);
}

internal class CommissionTagsRepository : WeakEntityRepository<CommissionTag, int, int>, ICommissionTagsRepository
{
    public CommissionTagsRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<CommissionTag>> GetAllForCommissionAsync(int commissionId)
    {
        IQueryable<CommissionTag> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);

        return await query.ToListAsync();
    }

    public override async Task<CommissionTag> GetByIdsAsync(int tagId, int commissionId)
    {
        return await base.GetByIdsAsync(tagId, commissionId);
    }
}
