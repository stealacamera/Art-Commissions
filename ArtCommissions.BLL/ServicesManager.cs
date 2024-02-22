using ArtCommissions.BLL.Services;
using ArtCommissions.BLL.Services.Singleton;
using ArtCommissions.BLL.Services.Transient;
using ArtCommissions.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.BLL;

public interface IServicesManager
{
    #region Services
    IAppUsersService AppUsersService { get; }
    ICommissionTagsService CommissionTagsService { get; }
    ITagsService TagsService { get; }
    ICommissionSampleImagesService CommissionSampleImagesService { get; }
    ICommissionsService CommissionsService { get; }
    IOrdersService OrdersService { get; }
    IFinalImagesService FinalImagesService { get; }
    IReviewsService ReviewsService { get; }
    IInvoicesService InvoicesService { get; }
    ICommissionReportsService CommissionReportsService { get; }
    IUserReportsService UserReportsService { get; }
    ILoggerService LoggerService { get; }
    IStripeService StripeService { get; }
    #endregion
    Task<T> WrapInTransactionAsync<T>(Func<Task<T>> asyncFunc);
}

internal class ServicesManager : IServicesManager
{
    private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

    private readonly IWebHostEnvironment _hostEnv;
    private readonly IWorkUnit _workUnit;
    private readonly IServiceProvider _serviceProvider;

    public ServicesManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _hostEnv = _serviceProvider.GetRequiredService<IWebHostEnvironment>();
        _workUnit = _serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public async Task<T> WrapInTransactionAsync<T>(Func<Task<T>> asyncFunc)
    {
        await _asyncLock.WaitAsync();

        using var transaction = await _workUnit.BeginTransactionAsync();
        T result;

        try
        {
            result = await asyncFunc();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            _asyncLock.Release();
        }

        return result;
    }

    private ITagsService _tagsService;
    public ITagsService TagsService
    {
        get
        {
            _tagsService ??= new TagsService(_serviceProvider);
            return _tagsService;
        }
    }

    private ICommissionTagsService _commissionTagsService;
    public ICommissionTagsService CommissionTagsService
    {
        get
        {
            _commissionTagsService ??= new CommissionTagsService(_serviceProvider);
            return _commissionTagsService;
        }
    }

    private IAppUsersService _appUsersService;
    public IAppUsersService AppUsersService
    {
        get
        {
            _appUsersService ??= new AppUsersService(_serviceProvider);
            return _appUsersService;
        }
    }

    private ICommissionSampleImagesService _commissionSampleImagesService;
    public ICommissionSampleImagesService CommissionSampleImagesService
    {
        get
        {
            _commissionSampleImagesService ??= new CommissionSampleImagesService(_serviceProvider);
            return _commissionSampleImagesService;
        }
    }

    private ICommissionsService _commissionsService;
    public ICommissionsService CommissionsService
    {
        get
        {
            _commissionsService ??= new CommissionsService(_serviceProvider);
            return _commissionsService;
        }
    }

    private IOrdersService _ordersService;
    public IOrdersService OrdersService
    {
        get
        {
            _ordersService ??= new OrdersService(_serviceProvider);
            return _ordersService;
        }
    }

    private IFinalImagesService _finalImagesService;
    public IFinalImagesService FinalImagesService
    {
        get
        {
            _finalImagesService ??= new FinalImagesService(_serviceProvider);
            return _finalImagesService;
        }
    }

    public IReviewsService _reviewsService;
    public IReviewsService ReviewsService
    {
        get
        {
            _reviewsService ??= new ReviewsService(_serviceProvider);
            return _reviewsService;
        }
    }

    private IInvoicesService _invoicesService;
    public IInvoicesService InvoicesService
    {
        get
        {
            _invoicesService ??= new InvoicesService(_serviceProvider);
            return _invoicesService;
        }
    }

    private ICommissionReportsService _commissionReportsService;

    public ICommissionReportsService CommissionReportsService
    {
        get
        {
            _commissionReportsService ??= new CommissionReportsService(_serviceProvider);
            return _commissionReportsService;
        }
    }

    private IUserReportsService _userReportsService;
    public IUserReportsService UserReportsService
    {
        get
        {
            _userReportsService ??= new UserReportsService(_serviceProvider);
            return _userReportsService;
        }
    }

    private ILoggerService _loggerService;
    public ILoggerService LoggerService
    {
        get
        {
            _loggerService ??= new LoggerService(_hostEnv);
            return _loggerService;
        }
    }

    private IStripeService _stripeService;
    public IStripeService StripeService
    {
        get
        {
            _stripeService ??= new StripeService(_serviceProvider);
            return _stripeService;
        }
    }
}
