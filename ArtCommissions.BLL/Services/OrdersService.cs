using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public delegate Task OrderConcludedByClientDelegate(IServiceProvider serviceProvider, AppUser vendor, string orderTitle, OrderStatus conclusionStatus);
public delegate Task OrderRejectedByVendorDelegate(IServiceProvider serviceProvider, AppUser client, string orderTitle);

public interface IOrdersService
{
    Task<PaginatedList<Order>> GetAllByClientAsync(int userId, OrderStatus status, int page, int pageSize);
    Task<PaginatedList<Order>> GetAllForVendorAsync(int vendorId, OrderStatus status, int page, int pageSize);
    Task<Order> GetByIdAsync(int id);
    Task<int> AddAsync(int userId, int commissionId, OrderAddRequestModel order);
    Task RemoveAsync(int userId, int id);
    Task CancelAsync(int id, int userId);
    Task FinishAsync(int id, int userId);
    Task SetInProgressAsync(int id, int userId);
    Task SetAsWaitingPaymentAsync(int id, int userId);
    Task<bool> DoesUserHaveOrderForCommissionAsync(int userId, int commissionId);
}

internal class OrdersService : IOrdersService
{
    public static event OrderConcludedByClientDelegate OrderConcludedByClient;
    public static event OrderRejectedByVendorDelegate OrderRejectedByVendor;

    private readonly IWorkUnit _workUnit;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServicesManager _servicesManager;

    public OrdersService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task<int> AddAsync(int userId, int commissionId, OrderAddRequestModel order)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(userId);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        if (userId == commission.Owner.Id)
            throw new UnauthorizedException();

        var dbModel = await _workUnit.OrdersRepository
                                     .AddAsync(new DAL.Entities.Order
                                     {
                                         Title = order.Title,
                                         Description = order.Description,
                                         ClientId = userId,
                                         CommissionId = commissionId,
                                         Status = OrderStatus.REQUEST
                                     });

        await _workUnit.SaveChangesAsync();
        return dbModel.Id;
    }

    public async Task<PaginatedList<Order>> GetAllByClientAsync(int clientId, OrderStatus status, int page, int pageSize)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(clientId);
        var dbResult = await _workUnit.OrdersRepository.GetAllByClientAsync(clientId, status, page, pageSize);

        PaginatedList<Order> result = new PaginatedList<Order>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(async e => new Order
                            {
                                Id = e.Id,
                                Title = e.Title,
                                TotalPrice = await _servicesManager.InvoicesService.GetTotalPaidPriceForOrderAsync(e.Id),
                                Client = new AppUser { Id = clientId },
                                Description = e.Description,
                                Status = status,
                                Commission = new Commission { Id = e.CommissionId }
                            })
                               .Select(e => e.Result)
                               .ToList()
        };

        return result;
    }

    public async Task<PaginatedList<Order>> GetAllForVendorAsync(int vendorId, OrderStatus status, int page, int pageSize)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(vendorId);
        var dbResult = await _workUnit.OrdersRepository.GetAllForVendorAsync(vendorId, status, page, pageSize);

        return new PaginatedList<Order>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(async e => new Order
                            {
                                Id = e.Id,
                                Title = e.Title,
                                TotalPrice = await _servicesManager.InvoicesService.GetTotalPaidPriceForOrderAsync(e.Id),
                                Client = new AppUser { Id = e.ClientId },
                                Commission = new Commission
                                {
                                    Id = e.CommissionId,
                                    Owner = new AppUser { Id = vendorId }
                                },
                                Description = e.Description,
                                Status = e.Status
                            })
                               .Select(e => e.Result)
                               .ToList()
        };
    }

    public async Task<Order> GetByIdAsync(int id)
    {
        var model = await _workUnit.OrdersRepository.GetByIdAsync(id);

        return new Order
        {
            Id = id,
            Title = model.Title,
            TotalPrice = await _servicesManager.InvoicesService.GetTotalPaidPriceForOrderAsync(id),
            Client = new AppUser { Id = model.ClientId },
            Commission = new Commission { Id = model.CommissionId },
            Description = model.Description,
            Status = model.Status
        };
    }

    public async Task RemoveAsync(int userId, int id)
    {
        var dbModel = await _workUnit.OrdersRepository.GetByIdAsync(id);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(dbModel.CommissionId);

        if (userId != dbModel.ClientId && userId != commission.Owner.Id)
            throw new UnauthorizedException();

        _workUnit.OrdersRepository.Remove(dbModel);
        await _workUnit.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var dbModel = await _workUnit.OrdersRepository.GetByIdAsync(id);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(dbModel.CommissionId);

        if (userId != dbModel.ClientId && userId != commission.Owner.Id)
            throw new UnauthorizedException();

        if (userId == commission.Owner.Id && (dbModel.Status != OrderStatus.REQUEST || await _servicesManager.InvoicesService.DoesOrderHaveOpenInvoiceAsync(id)))
            throw new ArtCommissionsException("You cannot reject an order that has already been accepted");

        dbModel.Status = OrderStatus.CANCELLED;
        await _workUnit.SaveChangesAsync();

        // Send notifying email
        if (userId == dbModel.ClientId)
        {
            var vendor = await _servicesManager.AppUsersService.GetByIdAsync(commission.Owner.Id);
            OrderConcludedByClient?.Invoke(_serviceProvider, vendor, dbModel.Title, OrderStatus.CANCELLED);
        }
        else
        {
            var client = await _servicesManager.AppUsersService.GetByIdAsync(dbModel.ClientId);
            OrderRejectedByVendor?.Invoke(_serviceProvider, client, dbModel.Title);
        }
    }

    public async Task FinishAsync(int id, int userId)
    {
        var dbModel = await _workUnit.OrdersRepository.GetByIdAsync(id);

        if (userId != dbModel.ClientId)
            throw new UnauthorizedException();

        if (dbModel.Status != OrderStatus.IN_PROGRESS)
            throw new ArtCommissionsException("You cannot conclude an order that isn't currently in progress");

        dbModel.Status = OrderStatus.FINISHED;
        await _workUnit.SaveChangesAsync();

        // Send notifying email
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(dbModel.CommissionId);
        var vendor = await _servicesManager.AppUsersService.GetByIdAsync(commission.Owner.Id);

        OrderConcludedByClient?.Invoke(_serviceProvider, vendor, dbModel.Title, OrderStatus.FINISHED);
    }

    public async Task SetInProgressAsync(int id, int userId)
    {
        var dbModel = await _workUnit.OrdersRepository.GetByIdAsync(id);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(dbModel.CommissionId);

        if (userId != dbModel.ClientId && userId != commission.Owner.Id)
            throw new UnauthorizedException();

        if (dbModel.Status != OrderStatus.WAITING_PAYMENT)
            throw new ArtCommissionsException("You cannot set in motion an order that is isn't currently expecting a payment");

        dbModel.Status = OrderStatus.IN_PROGRESS;
        await _workUnit.SaveChangesAsync();
    }

    public async Task SetAsWaitingPaymentAsync(int id, int userId)
    {
        var dbModel = await _workUnit.OrdersRepository.GetByIdAsync(id);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(dbModel.CommissionId);

        if (userId != commission.Owner.Id)
            throw new UnauthorizedException();

        if (dbModel.Status != OrderStatus.REQUEST && dbModel.Status != OrderStatus.IN_PROGRESS)
            throw new ArtCommissionsException("You cannot set as payment-pending an order that hasn't been accepted and/or is currently expecting a payment");

        dbModel.Status = OrderStatus.WAITING_PAYMENT;
        await _workUnit.SaveChangesAsync();
    }

    public async Task<bool> DoesUserHaveOrderForCommissionAsync(int userId, int commissionId)
    {
        return await _workUnit.OrdersRepository.DoesUserHaveOrderForCommissionAsync(userId, commissionId);
    }
}