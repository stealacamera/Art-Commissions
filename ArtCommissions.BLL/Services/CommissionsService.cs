using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public delegate Task AdminRemovedCommissionDelegate(IServiceProvider serviceProvider, AppUser vendor, Commission commission);

public interface ICommissionsService
{
    Task<PaginatedList<Commission>> FilterByKeywords(int page, int pageSize, string? keyword = null, int? tagId = null);
    Task<PaginatedList<Commission>> GetAllAsync(int page, int pageSize, bool excludeClosed = true);
    Task<PaginatedList<Commission>> GetAllByUserAsync(int userId, int page, int pageSize, bool excludeClosed = false);
    Task<Commission> GetByIdAsync(int id);
    Task<int> AddAsync(int userId, Commission commission);
    Task UpdateAsync(int userId, Commission commission);
    Task RemoveAsync(int userId, int id);
    Task UpdateStatusAsync(int userId, int id, bool newStatus);
}

internal class CommissionsService : ICommissionsService
{
    public static event AdminRemovedCommissionDelegate AdminRemovedCommission;

    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;
    private readonly IServiceProvider _serviceProvider;

    public CommissionsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task<int> AddAsync(int userId, Commission commission)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(userId);

        var dbModel = await _workUnit.CommissionsRepository
                                     .AddAsync(new DAL.Entities.Commission
                                     {
                                         OwnerId = userId,
                                         Description = commission.Description,
                                         MinPrice = commission.MinPrice,
                                         Title = commission.Title,
                                         IsClosed = commission.IsClosed
                                     });

        await _workUnit.SaveChangesAsync();
        return dbModel.Id;
    }

    public async Task<PaginatedList<Commission>> GetAllAsync(int page, int pageSize, bool excludeClosed = true)
    {
        var dbResult = await _workUnit.CommissionsRepository.GetAllAsync(page, pageSize, excludeClosed);

        PaginatedList<Commission> result = new PaginatedList<Commission>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new Commission
                            {
                                Id = e.Id,
                                Description = e.Description,
                                IsClosed = e.IsClosed,
                                MinPrice = e.MinPrice,
                                Title = e.Title,
                                Owner = new AppUser { Id = e.OwnerId }
                            })
                            .ToList()
        };

        return result;
    }

    public async Task<PaginatedList<Commission>> GetAllByUserAsync(int ownerId, int page, int pageSize, bool excludeClosed = false)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(ownerId);
        var dbResult = await _workUnit.CommissionsRepository.GetAllByUserAsync(ownerId, page, pageSize, excludeClosed);

        PaginatedList<Commission> result = new PaginatedList<Commission>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new Commission
                            {
                                Id = e.Id,
                                Description = e.Description,
                                IsClosed = e.IsClosed,
                                MinPrice = e.MinPrice,
                                Owner = new AppUser { Id = e.OwnerId },
                                Title = e.Title
                            })
                               .ToList()
        };

        return result;
    }

    public async Task<Commission> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.CommissionsRepository.GetByIdAsync(id);

        return new Commission
        {
            Id = dbModel.Id,
            Description = dbModel.Description,
            Owner = new AppUser { Id = dbModel.OwnerId },
            IsClosed = dbModel.IsClosed,
            MinPrice = dbModel.MinPrice,
            Title = dbModel.Title
        };
    }

    public async Task RemoveAsync(int userId, int id)
    {
        var dbModel = await _workUnit.CommissionsRepository.GetByIdAsync(id);

        // Only the owner or an admin can remove a commission
        if ((await _servicesManager.AppUsersService.IsInRole(userId, "User") && userId != dbModel.OwnerId)
            && !await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var ownerId = dbModel.OwnerId;
        var commission = new Commission
        {
            Id = dbModel.Id,
            Description = dbModel.Description,
            IsClosed = dbModel.IsClosed,
            MinPrice = dbModel.MinPrice,
            Title = dbModel.Title
        };

        _workUnit.CommissionsRepository.Remove(dbModel);
        await _workUnit.SaveChangesAsync();

        // Send notifying email if admin removed commission
        if (await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
        {
            var vendor = await _servicesManager.AppUsersService.GetByIdAsync(ownerId);
            AdminRemovedCommission?.Invoke(_serviceProvider, vendor, commission);
        }
    }

    public async Task UpdateAsync(int userId, Commission commission)
    {
        var dbModel = await _workUnit.CommissionsRepository.GetByIdAsync(commission.Id);

        if (userId != dbModel.OwnerId)
            throw new UnauthorizedException();

        dbModel.Title = commission.Title;
        dbModel.Description = commission.Description;
        dbModel.IsClosed = commission.IsClosed;
        dbModel.MinPrice = commission.MinPrice;

        await _workUnit.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int userId, int id, bool newStatus)
    {
        var dbModel = await _workUnit.CommissionsRepository.GetByIdAsync(id);

        if (userId != dbModel.OwnerId)
            throw new UnauthorizedException();

        dbModel.IsClosed = newStatus;
        await _workUnit.SaveChangesAsync();
    }

    public async Task<PaginatedList<Commission>> FilterByKeywords(int page, int pageSize, string? keyword = null, int? tagId = null)
    {
        var result = await _workUnit.CommissionsRepository.FilterByKeywords(page, pageSize, keyword, tagId);

        return new PaginatedList<Commission>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            Items = result.Items
                          .Select(async e => new Commission
                          {
                              Id = e.Id,
                              Description = e.Description,
                              IsClosed = e.IsClosed,
                              MinPrice = e.MinPrice,
                              Owner = new AppUser { Id = e.OwnerId },
                              Title = e.Title,
                              SampleImages = await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(e.Id)
                          })
                          .Select(e => e.Result)
                          .ToList()
        };
    }
}
