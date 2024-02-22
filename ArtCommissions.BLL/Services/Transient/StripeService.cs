using ArtCommissions.Common.Constants;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace ArtCommissions.BLL.Services.Transient;

public interface IStripeService
{
    Task<AppUserStripeProfile> CreateUserProfileAsync(AppUser user);
    Task<PaymentIntent> CreatePaymentIntent(int userId, int orderId, int invoiceId);
    Task CancelPaymentIntent(string id);
}

internal class StripeService : IStripeService
{
    private readonly IServicesManager _servicesManager;

    public StripeService(IServiceProvider serviceProvider)
    {
        _servicesManager = serviceProvider.GetRequiredService<IServicesManager>();
    }

    public async Task<AppUserStripeProfile> CreateUserProfileAsync(AppUser user)
    {
        return new AppUserStripeProfile
        {
            StripeCustomerId = (await CreateCustomer(user)).Id,
            StripeAccountId = (await CreateAccount(user)).Id
        };
    }

    public async Task CancelPaymentIntent(string id)
    {
        var service = new PaymentIntentService();
        await service.CancelAsync(id);
    }

    public async Task<PaymentIntent> CreatePaymentIntent(int userId, int orderId, int invoiceId)
    {
        var invoice = await _servicesManager.InvoicesService.GetByIdAsync(invoiceId);
        var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);

        if (userId != order.Client.Id)
            throw new UnauthorizedException();

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)invoice.Price * 1000,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                { StripeConstants.CLIENT_ID_METADATA, userId.ToString() },
                { StripeConstants.ORDER_ID_METADATA, orderId.ToString() },
                { StripeConstants.INVOICE_ID_METADATA, invoiceId.ToString() },
                { StripeConstants.INVOICE_PRICE_METADATA, invoice.Price.ToString() }
            }
        });

        return paymentIntent;
    }

    private async Task<Customer> CreateCustomer(AppUser user)
    {
        var stripeCustomerService = new CustomerService();
        return await stripeCustomerService.CreateAsync(new CustomerCreateOptions { Email = user.Email });
    }

    private async Task<Account> CreateAccount(AppUser user)
    {
        var accountOptions = new AccountCreateOptions
        {
            Type = "standard",
            Email = user.Email,
            BusinessType = "individual",
            BusinessProfile = new AccountBusinessProfileOptions
            {
                Mcc = "7333",
                Name = user.Username,
                SupportEmail = user.Email
            }
        };

        var accountService = new AccountService();
        var account = await accountService.CreateAsync(accountOptions);

        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = account.Id,
            Type = "account_onboarding",
            Collect = "eventually_due",
            RefreshUrl = "https://www.google.com",
            ReturnUrl = "https://www.google.com/"
        };

        var accountLinkService = new AccountLinkService();
        await accountLinkService.CreateAsync(accountLinkOptions);

        return account;
    }
}
