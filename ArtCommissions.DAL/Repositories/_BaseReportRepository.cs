using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;

namespace ArtCommissions.DAL.Repositories;

public interface IBaseReportRepository<TReport> : IStrongEntityRepository<TReport, int> where TReport : Report
{
    Task<PaginatedEnumerable<TReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize);
    void RemoveRange(IEnumerable<TReport> reports);
}

internal abstract class BaseReportRepository<TReport> : StrongEntityRepository<TReport, int>, IBaseReportRepository<TReport> where TReport : Report
{
    public BaseReportRepository(AppDbContext dbContext) : base(dbContext) { }

    public abstract Task<PaginatedEnumerable<TReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize);

    public void RemoveRange(IEnumerable<TReport> reports)
    {
        _set.RemoveRange(reports);
    }
}