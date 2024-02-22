using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface ICommissionSampleImagesService
{
    Task<List<CommissionSampleImage>> GetAllForCommissionAsync(int commissionId);
    Task AddAsync(int commissionId, string imagePath);
    Task RemoveAsync(int id);
    Task RemoveAllByCommissionAsync(int userId, int commissionId);
    Task<string> GetByIdAsync(int id);
}

internal class CommissionSampleImagesService : ICommissionSampleImagesService
{
    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;

    public CommissionSampleImagesService(IServiceProvider serviceProvider)
    {
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task<List<CommissionSampleImage>> GetAllForCommissionAsync(int commissionId)
    {
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        return (await _workUnit.CommissionSampleImagesRepository
                               .GetAllByCommissionAsync(commissionId))
                               .Select(e => new CommissionSampleImage { Id = e.Id, ImageName = e.ImagePath })
                               .ToList();
    }

    public async Task<string> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.CommissionSampleImagesRepository.GetByIdAsync(id);
        return dbModel.ImagePath;
    }

    public async Task AddAsync(int commissionId, string imagePath)
    {
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        await _workUnit.CommissionSampleImagesRepository
                       .AddAsync(new DAL.Entities.CommissionSampleImage
                       {
                           CommissionId = commissionId,
                           ImagePath = imagePath
                       });

        await _workUnit.SaveChangesAsync();
    }

    public async Task RemoveAsync(int id)
    {
        var dbModel = await _workUnit.CommissionSampleImagesRepository.GetByIdAsync(id);
        _workUnit.CommissionSampleImagesRepository.Remove(dbModel);

        await _workUnit.SaveChangesAsync();
    }

    public async Task RemoveAllByCommissionAsync(int userId, int commissionId)
    {
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        if (userId != commission.Owner.Id && !await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        await _workUnit.CommissionSampleImagesRepository.RemoveAllByCommissionAsync(commissionId);
        await _workUnit.SaveChangesAsync();
    }
}
