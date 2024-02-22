using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;

namespace ArtCommissions.BLL.Services;

public interface IUserReportsService : IBaseReportsService<UserReport, UserReportAddRequestModel>
{
    Task RemoveAllPendingForUserAsync(int userId, int reportedUserId);
}

internal class UserReportsService : BaseReportsService<UserReport, UserReportAddRequestModel>, IUserReportsService
{
    public UserReportsService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override async Task AddAsync(int userId, UserReportAddRequestModel model)
    {
        if (userId > 0)
        {
            if (userId == model.ReportedUserId)
                throw new ArtCommissionsException("You cannot report yourself");

            if (await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
                throw new UnauthorizedException();
        }

        await _workUnit.UserReportsRepository.AddAsync(new DAL.Entities.UserReport
        {
            ReportedUserId = model.ReportedUserId,
            Reason = model.Reason,
            Status = ReportStatus.PENDING
        });

        await _workUnit.SaveChangesAsync();
    }

    public override async Task<PaginatedList<UserReport>> GetAllByStatusAsync(ReportStatus status, int page, int pageSize)
    {
        var dbResult = await _workUnit.UserReportsRepository.GetAllByStatusAsync(status, page, pageSize);

        return new PaginatedList<UserReport>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new UserReport
                            {
                                Id = e.Id,
                                Reason = e.Reason,
                                Status = e.Status,
                                ReportedUser = new AppUser { Id = e.ReportedUserId }
                            })
                            .ToList()
        };
    }

    public override async Task<UserReport> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.UserReportsRepository.GetByIdAsync(id);

        return new UserReport
        {
            Id = dbModel.Id,
            Reason = dbModel.Reason,
            Status = dbModel.Status,
            ReportedUser = new AppUser { Id = dbModel.ReportedUserId }
        };
    }

    public override async Task UpdateStatusAsync(int id, ReportStatus newStatus, int userId)
    {
        await UpdateStatusPreconditionsAsync(newStatus, userId);

        var dbModel = await _workUnit.UserReportsRepository.GetByIdAsync(id);

        if (dbModel.Status != ReportStatus.PENDING)
            throw new ArtCommissionsException("Report status cannot be changed after being concluded");

        dbModel.Status = newStatus;
        await _workUnit.SaveChangesAsync();
    }

    public async Task RemoveAllPendingForUserAsync(int userId, int reportedUserId)
    {
        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var models = await _workUnit.UserReportsRepository.GetAllForUserAsync(reportedUserId, ReportStatus.PENDING);
        _workUnit.UserReportsRepository.RemoveRange(models);
    }
}
