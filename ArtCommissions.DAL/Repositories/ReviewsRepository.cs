using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IReviewsRepository : IWeakEntityRepository<Review, int, int>
{
    Task<Review?> GetByIdsOrDefaultAsync(int reviewerId, int commissionId);
    Task<PaginatedEnumerable<Review>> GetAllForCommissionAsync(int commissionId, int page, int pageSize);
    Task<decimal?> GetOverallScoreForUserAsync(int userId);
    Task RemoveAllForCommissionAsync(int commissionId);
    Task<decimal?> GetOverallScoreForCommissionAsync(int commissionId);
}

internal class ReviewsRepository : WeakEntityRepository<Review, int, int>, IReviewsRepository
{
    public ReviewsRepository(AppDbContext dbContext) : base(dbContext) { }

    public async override Task<Review> GetByIdsAsync(int reviewerId, int commissionId)
    {
        return await base.GetByIdsAsync(reviewerId, commissionId);
    }

    public async Task<PaginatedEnumerable<Review>> GetAllForCommissionAsync(int commissionId, int page, int pageSize)
    {
        IQueryable<Review> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);

        return await PaginatedEnumerable<Review>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<decimal?> GetOverallScoreForUserAsync(int userId)
    {
        IQueryable<Review> query = _untrackedSet;
        query = query.Where(e => e.Commission.OwnerId == userId);

        return (await query.AnyAsync()) ? (decimal)await query.Select(e => e.Rating).AverageAsync() : null;
    }

    public async Task RemoveAllForCommissionAsync(int commissionId)
    {
        IQueryable<Review> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);

        await query.ExecuteDeleteAsync();
    }

    public async Task<Review?> GetByIdsOrDefaultAsync(int reviewerId, int commissionId)
    {
        return await _set.FindAsync(reviewerId, commissionId);
    }

    public async Task<decimal?> GetOverallScoreForCommissionAsync(int commissionId)
    {
        IQueryable<Review> query = _untrackedSet;
        query = query.Where(e => e.CommissionId == commissionId);

        return (await query.AnyAsync()) ? (decimal)await query.Select(e => e.Rating).AverageAsync() : null;
    }
}
