using ArtCommissions.DAL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArtCommissions.DAL;

public interface IWorkUnit
{
    #region Repositories
    ITagsRepository TagsRepository { get; }
    ICommissionTagsRepository CommissionTagsRepository { get; }
    ICommissionsRepository CommissionsRepository { get; }
    ICommissionSampleImagesRepository CommissionSampleImagesRepository { get; }
    IOrdersRepository OrdersRepository { get; }
    IFinalImagesRepository FinalImagesRepository { get; }
    IInvoicesRepository InvoicesRepository { get; }
    IReviewsRepository ReviewsRepository { get; }
    IUserReportsRepository UserReportsRepository { get; }
    ICommissionReportsRepository CommissionReportsRepository { get; }
    #endregion

    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}

internal class WorkUnit : IWorkUnit
{
    private readonly AppDbContext _dbContext;

    public WorkUnit(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private ITagsRepository _tagsRepository;
    public ITagsRepository TagsRepository
    {
        get
        {
            _tagsRepository ??= new TagsRepository(_dbContext);
            return _tagsRepository;
        }
    }

    private ICommissionTagsRepository _commissionTagsRepository;
    public ICommissionTagsRepository CommissionTagsRepository
    {
        get
        {
            _commissionTagsRepository ??= new CommissionTagsRepository(_dbContext);
            return _commissionTagsRepository;
        }
    }

    private ICommissionsRepository _commissionsRepository;
    public ICommissionsRepository CommissionsRepository
    {
        get
        {
            _commissionsRepository ??= new CommissionsRepository(_dbContext);
            return _commissionsRepository;
        }
    }

    private ICommissionSampleImagesRepository _commissionSampleImagesRepository;
    public ICommissionSampleImagesRepository CommissionSampleImagesRepository
    {
        get
        {
            _commissionSampleImagesRepository ??= new CommissionSampleImagesRepository(_dbContext);
            return _commissionSampleImagesRepository;
        }
    }

    private IOrdersRepository _ordersRepository;
    public IOrdersRepository OrdersRepository
    {
        get
        {
            _ordersRepository ??= new OrdersRepository(_dbContext);
            return _ordersRepository;
        }
    }

    private IFinalImagesRepository _finalImagesRepository;
    public IFinalImagesRepository FinalImagesRepository
    {
        get
        {
            _finalImagesRepository ??= new FinalImagesRepository(_dbContext);
            return _finalImagesRepository;
        }
    }

    private IInvoicesRepository _invoicesRepository;
    public IInvoicesRepository InvoicesRepository
    {
        get
        {
            _invoicesRepository ??= new InvoicesRepository(_dbContext);
            return _invoicesRepository;
        }
    }

    private IReviewsRepository _reviewsRepository;
    public IReviewsRepository ReviewsRepository
    {
        get
        {
            _reviewsRepository ??= new ReviewsRepository(_dbContext);
            return _reviewsRepository;
        }
    }

    private IUserReportsRepository _userReportsRepository;
    public IUserReportsRepository UserReportsRepository
    {
        get
        {
            _userReportsRepository ??= new UserReportsRepository(_dbContext);
            return _userReportsRepository;
        }
    }

    private ICommissionReportsRepository _commissionReportsRepository;
    public ICommissionReportsRepository CommissionReportsRepository
    {
        get
        {
            _commissionReportsRepository ??= new CommissionReportsRepository(_dbContext);
            return _commissionReportsRepository;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }
}
