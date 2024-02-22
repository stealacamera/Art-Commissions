using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.DAOs;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtCommissions.DAL.Repositories;

public interface IOrdersRepository : IStrongEntityRepository<Order, int>
{
    Task<bool> DoesUserHaveOrderForCommissionAsync(int userId, int commissionId);
    Task<PaginatedEnumerable<Order>> GetAllByClientAsync(int clientId, OrderStatus status, int page, int pageSize);
    Task<PaginatedEnumerable<Order>> GetAllForVendorAsync(int vendorId, OrderStatus status, int page, int pageSize);
}

internal class OrdersRepository : StrongEntityRepository<Order, int>, IOrdersRepository
{
    public OrdersRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> DoesUserHaveOrderForCommissionAsync(int userId, int commissionId)
    {
        IQueryable<Order> query = _untrackedSet;
        query = query.Where(e => e.ClientId == userId && e.CommissionId == commissionId);

        return await query.AnyAsync();
    }

    public async Task<PaginatedEnumerable<Order>> GetAllByClientAsync(int clientId, OrderStatus status, int page, int pageSize)
    {
        IQueryable<Order> query = _untrackedSet;

        query = query.Where(e => e.ClientId == clientId);
        query = query.Where(e => e.Status == status);

        return await PaginatedEnumerable<Order>.CreatePaginatedResponseAsync(query, page, pageSize);
    }

    public async Task<PaginatedEnumerable<Order>> GetAllForVendorAsync(int vendorId, OrderStatus status, int page, int pageSize)
    {
        IQueryable<Order> query = _untrackedSet;

        query = query.Where(e => e.Commission.OwnerId == vendorId);
        query = query.Where(e => e.Status == status);

        return await PaginatedEnumerable<Order>.CreatePaginatedResponseAsync(query, page, pageSize);
    }
}
