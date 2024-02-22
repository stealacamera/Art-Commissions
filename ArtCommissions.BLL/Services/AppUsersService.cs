using ArtCommissions.Common.Exceptions;
using ArtCommissions.Common.DTOs;
using Microsoft.AspNetCore.Identity;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public delegate Task UserLockedOutDelegate(IServiceProvider serviceProvider, AppUser user);

public interface IAppUsersService
{
    Task<bool> IsLockedOut(int userId);
    Task<bool> IsInRole(int id, string roleName);
    Task AddSuspensionStrikeAsync(int maxNrSuspensionStrikes, int userId, int id);
    Task LockoutAsync(int maxNrSuspensionStrikes, int userId, int id);
    Task<AppUser> GetByIdAsync(int id);
    Task AddAdminstrativeUserAsync(int userId, AppUserAddRequestModel model);
    Task<AppUserStripeProfile> GetStripeProfileAsync(int id);
}
internal class AppUsersService : IAppUsersService
{
    public static event UserLockedOutDelegate UserLockedOut;

    private readonly IWorkUnit _workUnit;
    private readonly RoleManager<DAL.Entities.AppRole> _roleManager;
    private readonly UserManager<DAL.Entities.AppUser> _userManager;
    private readonly IServiceProvider _serviceProvider;

    public AppUsersService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _workUnit = _serviceProvider.GetRequiredService<IWorkUnit>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<DAL.Entities.AppUser>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<DAL.Entities.AppRole>>();
    }

    private async Task<DAL.Entities.AppUser> GetEntityByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString()) ?? throw new EntityNotFoundException(entityName: "User");
    }

    public async Task<bool> IsInRole(int id, string roleName)
    {
        var dbModel = await GetEntityByIdAsync(id);
        return await _userManager.IsInRoleAsync(dbModel, roleName);
    }

    public async Task<AppUser> GetByIdAsync(int id)
    {
        var dbModel = await GetEntityByIdAsync(id);

        return new AppUser
        {
            Id = dbModel.Id,
            Username = dbModel.UserName,
            Email = dbModel.Email
        };
    }

    public async Task<AppUserStripeProfile> GetStripeProfileAsync(int id)
    {
        var dbModel = await GetEntityByIdAsync(id);

        return new AppUserStripeProfile
        {
            StripeCustomerId = dbModel.StripeCustomerId,
            StripeAccountId = dbModel.StripeAccountId
        };
    }

    public async Task AddSuspensionStrikeAsync(int maxNrSuspensionStrikes, int userId, int id)
    {
        if (userId == id)
            throw new ArtCommissionsException("You cannot add a strike to yourself");

        if (!await IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var dbModel = await GetEntityByIdAsync(id);

        if (dbModel.NrSuspensionStrikes < maxNrSuspensionStrikes)
        {
            dbModel.NrSuspensionStrikes++;

            await _userManager.UpdateAsync(dbModel);
            await _workUnit.SaveChangesAsync();
        }
        else
            await SetLockedOutAsync(maxNrSuspensionStrikes, dbModel);
    }

    public async Task LockoutAsync(int maxNrSuspensionStrikes, int userId, int id)
    {
        if (userId == id)
            throw new ArtCommissionsException("You cannot lock out yourself");

        if (!await IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        var dbModel = await GetEntityByIdAsync(id);
        await SetLockedOutAsync(maxNrSuspensionStrikes, dbModel);
    }

    private async Task SetLockedOutAsync(int maxNrSuspensionStrikes, DAL.Entities.AppUser dbModel)
    {
        dbModel.NrSuspensionStrikes = maxNrSuspensionStrikes;
        dbModel.LockoutEnd = DateTime.MaxValue;

        await _userManager.UpdateAsync(dbModel);
        await _workUnit.SaveChangesAsync();

        UserLockedOut?.Invoke(
            _serviceProvider,
            new AppUser
            {
                Id = dbModel.Id,
                Username = dbModel.UserName,
                Email = dbModel.Email
            });
    }

    public async Task AddAdminstrativeUserAsync(int userId, AppUserAddRequestModel model)
    {
        if (model.Role == "User")
            throw new ArtCommissionsException("You cannot add a new basic user");

        if (!await IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        if (!await _roleManager.RoleExistsAsync(model.Role))
            throw new ArtCommissionsException($"The \"{model.Role}\" role doesn't exist");

        var dbModel = new DAL.Entities.AppUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = true,
            StripeCustomerId = model.StripeCustomerId,
            StripeAccountId = model.StripeAccountId
        };

        var result = await _userManager.CreateAsync(dbModel, model.Password);

        if (result.Succeeded)
            await _userManager.AddToRoleAsync(dbModel, model.Role);
        else
            throw new ArtCommissionsException(result.Errors.First().Description);
    }

    public async Task<bool> IsLockedOut(int userId)
    {
        var model = await GetEntityByIdAsync(userId);
        return await _userManager.IsLockedOutAsync(model);
    }
}
