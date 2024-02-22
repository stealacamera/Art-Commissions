using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface ITagsService
{
    Task<PaginatedList<Tag>> GetAllAsync(int page, int pageSize);
    Task<List<Tag>> GetAllAsync();
    Task<Tag> GetByIdAsync(int id);
    Task<Tag> AddAsync(int userId, Tag tag);
    Task EditAsync(int userId, Tag tag);
    Task RemoveAsync(int userId, int id);
}

internal class TagsService : ITagsService
{
    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;
    private readonly IMemoryCache _memoryCache;

    private const string TAGS_CACHE_KEY = "_tags_";

    public TagsService(IServiceProvider serviceProvider)
    {
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    }

    public async Task<Tag> AddAsync(int userId, Tag tag)
    {
        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var dbModel = await _workUnit.TagsRepository.AddAsync(new DAL.Entities.Tag { Name = tag.Name });

        await _workUnit.SaveChangesAsync();
        _memoryCache.Remove(TAGS_CACHE_KEY);

        return new Tag { Id = dbModel.Id, Name = tag.Name };
    }

    public async Task EditAsync(int userId, Tag tag)
    {
        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var dbModel = await _workUnit.TagsRepository.GetByIdAsync(tag.Id);
        dbModel.Name = tag.Name;

        await _workUnit.SaveChangesAsync();
    }

    public async Task<PaginatedList<Tag>> GetAllAsync(int page, int pageSize)
    {
        var result = await _workUnit.TagsRepository.GetAllAsync(page, pageSize);

        return new PaginatedList<Tag>
        {
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = pageSize,
            Items = result.Items.Select(e => new Tag { Id = e.Id, Name = e.Name }).ToList()
        };
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        var result = await _memoryCache.GetOrCreateAsync(
            TAGS_CACHE_KEY,
            async entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                entry.SlidingExpiration = TimeSpan.FromHours(5);

                return (await _workUnit.TagsRepository
                               .GetAllAsync())
                               .Select(e => new Tag { Id = e.Id, Name = e.Name })
                               .ToList();
            });

        return result;
    }

    public async Task<Tag> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.TagsRepository.GetByIdAsync(id);
        return new Tag { Id = dbModel.Id, Name = dbModel.Name };
    }

    public async Task RemoveAsync(int userId, int id)
    {
        if (!await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var dbModel = await _workUnit.TagsRepository.GetByIdAsync(id);
        _workUnit.TagsRepository.Remove(dbModel);

        await _workUnit.SaveChangesAsync();
        _memoryCache.Remove(TAGS_CACHE_KEY);
    }
}
