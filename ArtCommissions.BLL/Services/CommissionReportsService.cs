using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;

namespace ArtCommissions.BLL.Services;

public interface ICommissionReportsService : IBaseReportsService<CommissionReport, CommissionReportAddRequestModel>
{
    Task RemoveAllPendingForCommissionAsync(int userId, int reportedCommissionId);
}

internal class CommissionReportsService : BaseReportsService<CommissionReport, CommissionReportAddRequestModel>, ICommissionReportsService
{
    public CommissionReportsService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task AddAsync(int userId, CommissionReportAddRequestModel model)
    {
        if (userId > 0)
        {
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(model.ReportedCommissionId);

            if (userId == commission.Owner.Id)
                throw new ArtCommissionsException("You cannot report your own commission");

            if (await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
                throw new UnauthorizedException();
        }

        await _workUnit.CommissionReportsRepository.AddAsync(new DAL.Entities.CommissionReport
        {
            ReportedCommissionId = model.ReportedCommissionId,
            Reason = model.Reason,
            Status = ReportStatus.PENDING
        });

        await _workUnit.SaveChangesAsync();
    }

    public override async Task<PaginatedList<CommissionReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize)
    {
        var dbResult = await _workUnit.CommissionReportsRepository.GetAllByStatusAsync(status, page, pageSize);

        return new PaginatedList<CommissionReport>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new CommissionReport
                            {
                                Id = e.Id,
                                Reason = e.Reason,
                                ReportedCommission = new Commission { Id = e.ReportedCommissionId },
                                Status = e.Status
                            })
                            .ToList(),
        };
    }

    public override async Task<CommissionReport> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.CommissionReportsRepository.GetByIdAsync(id);

        return new CommissionReport
        {
            Id = dbModel.Id,
            Reason = dbModel.Reason,
            Status = dbModel.Status,
            ReportedCommission = new Commission { Id = dbModel.ReportedCommissionId },
        };
    }

    public override async Task UpdateStatusAsync(int id, ReportStatus newStatus, int userId)
    {
        await UpdateStatusPreconditionsAsync(newStatus, userId);

        var dbModel = await _workUnit.CommissionReportsRepository.GetByIdAsync(id);

        if (dbModel.Status != ReportStatus.PENDING)
            throw new ArtCommissionsException("Report status cannot be changed after being concluded");

        dbModel.Status = newStatus;
        await _workUnit.SaveChangesAsync();
    }

    public async Task RemoveAllPendingForCommissionAsync(int userId, int reportedCommissionId)
    {
        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var models = await _workUnit.CommissionReportsRepository.GetAllForCommissionAsync(reportedCommissionId, ReportStatus.PENDING);
        _workUnit.CommissionReportsRepository.RemoveRange(models);
    }
}
