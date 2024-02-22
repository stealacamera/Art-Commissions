using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface ICommissionReportsRepository : IStrongEntityRepository<CommissionReport, int>, IBaseReportRepository<CommissionReport>
{
    Task<IEnumerable<CommissionReport>> GetAllForCommissionAsync(int commissionId, ReportStatus? status = null);
}

internal class CommissionReportsRepository : BaseReportRepository<CommissionReport>, ICommissionReportsRepository
{
    public CommissionReportsRepository(AppDbContext dbContext) : base(dbContext) { }

    public override async Task<PaginatedEnumerable<CommissionReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize)
    {
        IQueryable<CommissionReport> query = _untrackedSet;
        query = query.Where(e => e.Status == status);

        return await PaginatedEnumerable<CommissionReport>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<IEnumerable<CommissionReport>> GetAllForCommissionAsync(int commissionId, ReportStatus? status = null)
    {
        IQueryable<CommissionReport> query = _untrackedSet;
        query = query.Where(e => e.ReportedCommissionId == commissionId);

        if(status != null)
            query = query.Where(e => e.Status == status);

        return await query.ToListAsync();
    }
}
