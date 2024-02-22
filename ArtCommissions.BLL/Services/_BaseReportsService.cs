using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface IBaseReportsService<TReportModel, TReportAddRequestModel> where TReportModel : Report
                                                                           where TReportAddRequestModel : ReportAddRequestModel
{
    Task AddAsync(int userId, TReportAddRequestModel model);
    Task UpdateStatusAsync(int id, ReportStatus newStatus, int userId);
    Task<TReportModel> GetByIdAsync(int id);
    Task<PaginatedList<TReportModel>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize);
}

public abstract class BaseReportsService<TReportModel, TReportAddRequestModel> : IBaseReportsService<TReportModel, TReportAddRequestModel> where TReportModel : Report
                                                                                                                                           where TReportAddRequestModel : ReportAddRequestModel
{
    protected readonly IWorkUnit _workUnit;
    protected readonly IServicesManager _servicesManager;
    protected readonly IServiceProvider _serviceProvider;

    public BaseReportsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _servicesManager = _serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = _serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public abstract Task AddAsync(int userId, TReportAddRequestModel model);

    public abstract Task<PaginatedList<TReportModel>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize);

    public abstract Task<TReportModel> GetByIdAsync(int id);

    public abstract Task UpdateStatusAsync(int id, ReportStatus newStatus, int userId);

    protected async Task UpdateStatusPreconditionsAsync(ReportStatus newStatus, int userId)
    {
        if (newStatus == ReportStatus.PENDING)
            throw new ArtCommissionsException("Report cannot be changed back to pending after being concluded");

        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();
    }
}