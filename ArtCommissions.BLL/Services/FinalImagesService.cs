using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface IFinalImagesService
{
    Task AddAsync(int userId, int orderId, string imagePath);
    Task RemoveForOrderAsync(int userId, int orderId);
    Task<string?> GetByOrderAsync(int orderId);
}

internal class FinalImagesService : IFinalImagesService
{
    private readonly IServicesManager _servicesManager;
    private readonly IWorkUnit _workUnit;

    public FinalImagesService(IServiceProvider serviceProvider)
    {
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task AddAsync(int userId, int orderId, string imagePath)
    {
        if (await _workUnit.FinalImagesRepository.GetByOrderAsync(orderId) != null)
            throw new ArtCommissionsException("You cannot upload a new final image without removing the existing one for this order");

        var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);

        if (order.Status != OrderStatus.IN_PROGRESS)
            throw new ArtCommissionsException("You cannot add a new final image to an order not currently in progress");

        var commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);

        if (userId != commission.Owner.Id)
            throw new UnauthorizedException();

        await _workUnit.FinalImagesRepository
                       .AddAsync(new DAL.Entities.FinalImage
                       {
                           ImagePath = imagePath,
                           OrderId = orderId
                       });

        await _workUnit.SaveChangesAsync();
    }

    public async Task<string?> GetByOrderAsync(int orderId)
    {
        var dbModel = await _workUnit.FinalImagesRepository.GetByOrderAsync(orderId);
        return dbModel != null ? dbModel.ImagePath : null;
    }

    public async Task RemoveForOrderAsync(int userId, int orderId)
    {
        var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);

        if (userId != commission.Owner.Id)
            throw new UnauthorizedException();

        var dbModel = await _workUnit.FinalImagesRepository.GetByOrderAsync(orderId);

        if (dbModel != null)
        {
            _workUnit.FinalImagesRepository.Remove(dbModel);
            await _workUnit.SaveChangesAsync();
        }
    }
}
