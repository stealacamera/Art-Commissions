using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface ICommissionTagsService
{
    Task<List<Tag>> GetAllForCommissionAsync(int commissionId);
    Task AddAsync(int userId, int commissionId, int tagId);
    Task RemoveAsync(int userId, int commissionId, int tagId);
}

internal class CommissionTagsService : ICommissionTagsService
{
    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;

    public CommissionTagsService(IServiceProvider serviceProvider)
    {
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
    }

    public async Task AddAsync(int userId, int commissionId, int tagId)
    {
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);
        _ = await _servicesManager.TagsService.GetByIdAsync(tagId);

        if (userId != commission.Owner.Id)
            throw new UnauthorizedException();

        await _workUnit.CommissionTagsRepository
                       .AddAsync(new DAL.Entities.CommissionTag
                       {
                           CommissionId = commissionId,
                           TagId = tagId
                       });

        await _workUnit.SaveChangesAsync();
    }

    public async Task<List<Tag>> GetAllForCommissionAsync(int commissionId)
    {
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);
        var result = await _workUnit.CommissionTagsRepository.GetAllForCommissionAsync(commissionId);

        return result.Select(async e => await _servicesManager.TagsService.GetByIdAsync(e.TagId))
                     .Select(e => e.Result)
                     .ToList();
    }

    public async Task RemoveAsync(int userId, int commissionId, int tagId)
    {
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);
        _ = await _servicesManager.TagsService.GetByIdAsync(tagId);

        if (userId != commission.Owner.Id)
            throw new UnauthorizedException();

        var dbModel = await _workUnit.CommissionTagsRepository.GetByIdsAsync(tagId, commissionId);
        _workUnit.CommissionTagsRepository.Remove(dbModel);

        await _workUnit.SaveChangesAsync();
    }
}
