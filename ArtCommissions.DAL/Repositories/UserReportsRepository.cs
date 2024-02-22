using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IUserReportsRepository : IStrongEntityRepository<UserReport, int>, IBaseReportRepository<UserReport>
{
    Task<IEnumerable<UserReport>> GetAllForUserAsync(int userId, ReportStatus? status = null);
}

internal class UserReportsRepository : BaseReportRepository<UserReport>, IUserReportsRepository
{
    public UserReportsRepository(AppDbContext dbContext) : base(dbContext) { }

    public override async Task<PaginatedEnumerable<UserReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize)
    {
        IQueryable<UserReport> query = _untrackedSet;
        query = query.Where(e => e.Status == status);

        return await PaginatedEnumerable<UserReport>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<IEnumerable<UserReport>> GetAllForUserAsync(int userId, ReportStatus? status = null)
    {
        IQueryable<UserReport> query = _untrackedSet;
        query = query.Where(e => e.ReportedUserId == userId);

        if (status != null)
            query = query.Where(e => e.Status == status);

        return await query.ToListAsync();
    }
}