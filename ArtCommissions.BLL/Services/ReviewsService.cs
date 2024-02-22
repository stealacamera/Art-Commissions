using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL.Services;

public interface IReviewsService
{
    Task AddAsync(int commissionId, int reviewerId, ReviewAddRequestModel review);
    Task RemoveAsync(int reviewerId, int commissionId);
    Task<decimal?> GetOverallScoreForUserAsync(int userId);
    Task<PaginatedList<Review>> GetAllForCommissionAsync(int commissionId, int page, int pageSize);
    Task RemoveAllForCommissionAsync(int userId, int commissionId);
    Task<decimal?> GetOverallScoreForCommissionAsync(int commissionId);
}

internal class ReviewsService : IReviewsService
{
    private readonly IWorkUnit _workUnit;
    private readonly IServicesManager _servicesManager;

    public ReviewsService(IServiceProvider serviceProvider)
    {
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
        _workUnit = serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task AddAsync(int commissionId, int reviewerId, ReviewAddRequestModel review)
    {
        if (await _workUnit.ReviewsRepository.GetByIdsOrDefaultAsync(reviewerId, commissionId) != null)
            throw new ExistingReviewForCommissionException();

        if (!await _servicesManager.OrdersService.DoesUserHaveOrderForCommissionAsync(reviewerId, commissionId))
            throw new ArtCommissionsException("You cannot review a commission you haven't placed an order on");

        _ = await _servicesManager.AppUsersService.GetByIdAsync(reviewerId);
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        await _workUnit.ReviewsRepository
                       .AddAsync(new DAL.Entities.Review
                       {
                           CommissionId = commissionId,
                           ReviewerId = reviewerId,
                           Description = review.Description,
                           Rating = review.Rating,
                           Title = review.Title
                       });

        await _workUnit.SaveChangesAsync();
    }

    public async Task<PaginatedList<Review>> GetAllForCommissionAsync(int commissionId, int page, int pageSize)
    {
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);
        var dbResult = await _workUnit.ReviewsRepository.GetAllForCommissionAsync(commissionId, page, pageSize);

        PaginatedList<Review> result = new PaginatedList<Review>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = dbResult.TotalCount,
            Items = dbResult.Items
                            .Select(e => new Review
                            {
                                Commission = new Commission { Id = e.CommissionId },
                                Description = e.Description,
                                Rating = e.Rating,
                                Reviewer = new AppUser { Id = e.ReviewerId },
                                Title = e.Title
                            })
                               .ToList()
        };

        return result;
    }

    public async Task<decimal?> GetOverallScoreForCommissionAsync(int commissionId)
    {
        _ = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);
        return await _workUnit.ReviewsRepository.GetOverallScoreForCommissionAsync(commissionId);
    }

    public async Task<decimal?> GetOverallScoreForUserAsync(int userId)
    {
        _ = await _servicesManager.AppUsersService.GetByIdAsync(userId);
        return await _workUnit.ReviewsRepository.GetOverallScoreForUserAsync(userId);
    }

    public async Task RemoveAllForCommissionAsync(int userId, int commissionId)
    {
        var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

        if (userId != commission.Owner.Id && !await _servicesManager.AppUsersService.IsInRole(userId, "Admin"))
            throw new UnauthorizedException();

        await _workUnit.ReviewsRepository.RemoveAllForCommissionAsync(commissionId);
        await _workUnit.SaveChangesAsync();
    }

    public async Task RemoveAsync(int reviewerId, int commissionId)
    {
        var dbModel = await _workUnit.ReviewsRepository.GetByIdsAsync(reviewerId, commissionId) ?? throw new EntityNotFoundException(entityName: "Review");
        _workUnit.ReviewsRepository.Remove(dbModel);

        await _workUnit.SaveChangesAsync();
    }
}
