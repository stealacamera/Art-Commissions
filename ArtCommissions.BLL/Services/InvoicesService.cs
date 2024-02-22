using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public delegate Task InvoiceAddedDelegate(IServiceProvider serviceProvider, AppUser client, Order order, bool hasPreviousInvoiceHistory);
public delegate Task InvoiceUpdatedDelegate(IServiceProvider serviceProvider, AppUser client, Order order);
public delegate Task InvoicePaidDelegate(IServiceProvider serviceProvider, AppUser vendor, Order order);

public interface IInvoicesService
{
    Task<Invoice> GetByIdAsync(int id);
    Task AddAsync(InvoiceUpsertRequestModel invoice, int userId);
    Task UpdateAsync(InvoiceUpsertRequestModel invoice, int userId);
    Task PayAsync(int id, int userId);
    Task CancelAsync(int id, int orderId, int userId);
    Task<PaginatedList<Invoice>> GetAllForOrderAsync(int orderId, int page, int pageSize);
    Task RemoveAllForOrderAsync(int userId, int orderId);
    Task<decimal> GetTotalPaidPriceForOrderAsync(int orderId);
    Task<bool> DoesOrderHaveOpenInvoiceAsync(int orderId);
}

internal class InvoicesService : IInvoicesService
{
    public static event InvoiceAddedDelegate InvoiceAdded;
    public static event InvoiceUpdatedDelegate InvoiceUpdated;
    public static event InvoicePaidDelegate InvoicePaid;

    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;
    private readonly IServiceProvider _serviceProvider;

    public InvoicesService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    private async Task<bool> IsUserVendorOfOrderAsync(int userId, int orderId)
    {
        var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);

        return userId == commission.Owner.Id;
    }

    public async Task<Invoice> GetByIdAsync(int id)
    {
        var dbModel = await _workUnit.InvoicesRepository.GetByIdAsync(id);

        return new Invoice
        {
            Id = dbModel.Id,
            CreatedAt = dbModel.CreatedAt,
            Description = dbModel.Description,
            PayedAt = dbModel.PayedAt,
            Price = dbModel.Price,
            Status = dbModel.Status,
            Title = dbModel.Title,
            Order = new Order { Id = dbModel.OrderId }
        };
    }

    public async Task AddAsync(InvoiceUpsertRequestModel invoice, int userId)
    {
        if (!await IsUserVendorOfOrderAsync(userId, invoice.OrderId))
            throw new UnauthorizedException();

        if (await _workUnit.InvoicesRepository.DoesOrderHaveOpenInvoiceAsync(invoice.OrderId))
            throw new ExistingUnpaidInvoiceException();

        bool hasPreviousInvoices = await _workUnit.InvoicesRepository.DoesOrderHavePreviousInvoicesAsync(invoice.OrderId);

        await _workUnit.InvoicesRepository.AddAsync(new DAL.Entities.Invoice
        {
            OrderId = invoice.OrderId,
            Title = invoice.Title,
            Description = invoice.Description,
            Price = invoice.Price,
            Status = InvoiceStatus.WAITING_PAYMENT,
            CreatedAt = DateTime.Now
        });

        await _workUnit.SaveChangesAsync();

        // Send notifying email
        var order = await _servicesManager.OrdersService.GetByIdAsync(invoice.OrderId);
        var client = await _servicesManager.AppUsersService.GetByIdAsync(order.Client.Id);

        InvoiceAdded?.Invoke(_serviceProvider, client, order, hasPreviousInvoices);
    }

    public async Task<PaginatedList<Invoice>> GetAllForOrderAsync(int orderId, int page, int pageSize)
    {
        _ = await _servicesManager.OrdersService.GetByIdAsync(orderId);
        var dbResult = await _workUnit.InvoicesRepository.GetAllByOrderAsync(orderId, page, pageSize);

        PaginatedList<Invoice> result = new PaginatedList<Invoice>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new Invoice
                            {
                                Id = e.Id,
                                CreatedAt = e.CreatedAt,
                                Description = e.Description,
                                Status = e.Status,
                                PayedAt = e.PayedAt,
                                Price = e.Price,
                                Title = e.Title,
                                Order = new Order { Id = e.OrderId }
                            })
                               .ToList()
        };

        return result;
    }

    public async Task UpdateAsync(InvoiceUpsertRequestModel invoice, int userId)
    {
        if (!await IsUserVendorOfOrderAsync(userId, invoice.OrderId))
            throw new UnauthorizedException();

        var dbModel = await _workUnit.InvoicesRepository.GetByIdAsync(invoice.Id);

        if (dbModel.Status != InvoiceStatus.WAITING_PAYMENT)
            throw new ArtCommissionsException("You cannot change an invoice that has already been paid or cancelled");

        dbModel.Title = invoice.Title;
        dbModel.Description = invoice.Description;
        dbModel.Price = invoice.Price;

        await _workUnit.SaveChangesAsync();

        // Send notifying email
        var order = await _servicesManager.OrdersService.GetByIdAsync(invoice.OrderId);
        var client = await _servicesManager.AppUsersService.GetByIdAsync(order.Client.Id);

        InvoiceUpdated?.Invoke(_serviceProvider, client, order);
    }

    public async Task PayAsync(int id, int userId)
    {
        var dbModel = await _workUnit.InvoicesRepository.GetByIdAsync(id);
        var order = await _servicesManager.OrdersService.GetByIdAsync(dbModel.OrderId);

        if (userId != order.Client.Id)
            throw new UnauthorizedException();

        if (dbModel.Status != InvoiceStatus.WAITING_PAYMENT)
            throw new ArtCommissionsException("You cannot pay a cancelled or an already paid invoice.");

        dbModel.Status = InvoiceStatus.PAID;
        dbModel.PayedAt = DateTime.Now;
        await _workUnit.SaveChangesAsync();

        // Send notifying email
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);
        var vendor = await _servicesManager.AppUsersService.GetByIdAsync(commission.Owner.Id);

        InvoicePaid?.Invoke(_serviceProvider, vendor, order);
    }

    public async Task<decimal> GetTotalPaidPriceForOrderAsync(int orderId)
    {
        return await _workUnit.InvoicesRepository.GetTotalPaidPriceForOrderAsync(orderId);
    }

    public async Task CancelAsync(int id, int orderId, int userId)
    {
        if (!await IsUserVendorOfOrderAsync(userId, orderId))
            throw new UnauthorizedException();

        var dbModel = await _workUnit.InvoicesRepository.GetByIdAsync(id);

        if (dbModel.Status != InvoiceStatus.WAITING_PAYMENT)
            throw new ArtCommissionsException("You cannot cancel a paid or already cancelled invoice");

        if (!await _workUnit.InvoicesRepository.DoesOrderHavePreviousInvoicesAsync(dbModel.OrderId, id))
            throw new EmptyInvoiceHistoryException("You cannot cancel an invoice on a newly accepted order. You can reject/cancel the order instead.");

        dbModel.Status = InvoiceStatus.CANCELLED;
        await _workUnit.SaveChangesAsync();
    }

    public async Task<bool> DoesOrderHaveOpenInvoiceAsync(int orderId)
    {
        _ = await _servicesManager.OrdersService.GetByIdAsync(orderId);
        return await _workUnit.InvoicesRepository.DoesOrderHaveOpenInvoiceAsync(orderId);
    }

    public async Task RemoveAllForOrderAsync(int userId, int orderId)
    {
        var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);

        if (userId != order.Client.Id)
            throw new UnauthorizedException();

        await _workUnit.InvoicesRepository.RemoveAllForOrderAsync(orderId);
        await _workUnit.SaveChangesAsync();
    }
}