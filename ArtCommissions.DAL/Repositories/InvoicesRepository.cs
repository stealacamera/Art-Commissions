using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IInvoicesRepository : IStrongEntityRepository<Invoice, int>
{
    Task<PaginatedEnumerable<Invoice>> GetAllByOrderAsync(int orderId, int page, int pageSize);
    Task<bool> DoesOrderHavePreviousInvoicesAsync(int orderId, int? invoiceIdToExclude = null);
    Task<bool> DoesOrderHaveOpenInvoiceAsync(int orderId);
    Task<decimal> GetTotalPaidPriceForOrderAsync(int orderId);
    Task RemoveAllForOrderAsync(int orderId);
}

internal class InvoicesRepository : StrongEntityRepository<Invoice, int>, IInvoicesRepository
{
    public InvoicesRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> DoesOrderHavePreviousInvoicesAsync(int orderId, int? invoiceIdToExclude = null)
    {
        IQueryable<Invoice> query = _untrackedSet;
        query = query.Where(e => e.OrderId == orderId);

        if (invoiceIdToExclude != null)
            query = query.Where(e => e.Id != invoiceIdToExclude);

        return await query.AnyAsync();
    }

    public async Task<PaginatedEnumerable<Invoice>> GetAllByOrderAsync(int orderId, int page, int pageSize)
    {
        IQueryable<Invoice> query = _untrackedSet;

        query = query.OrderByDescending(e => e.CreatedAt);
        query = query.Where(e => e.OrderId == orderId);

        return await PaginatedEnumerable<Invoice>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<decimal> GetTotalPaidPriceForOrderAsync(int orderId)
    {
        IQueryable<Invoice> query = _untrackedSet;

        query = query.Where(e => e.OrderId == orderId);
        query = query.Where(e => e.Status == InvoiceStatus.PAID);

        return await query.AnyAsync() ? await query.Select(e => e.Price).SumAsync() : 0;
    }

    public async Task<bool> DoesOrderHaveOpenInvoiceAsync(int orderId)
    {
        IQueryable<Invoice> query = _set;

        query = query.Where(e => e.OrderId == orderId);
        query = query.Where(e => e.Status == InvoiceStatus.WAITING_PAYMENT);

        return await query.AnyAsync();
    }

    public async Task RemoveAllForOrderAsync(int orderId)
    {
        IQueryable<Invoice> query = _untrackedSet;
        query = query.Where(e => e.OrderId == orderId);

        await query.ExecuteDeleteAsync();
    }
}