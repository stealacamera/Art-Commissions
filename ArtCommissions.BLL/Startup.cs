using ArtCommissions.BLL.Services.Hosted;
using ArtCommissions.BLL.Services.Singleton;
using ArtCommissions.BLL.Services.Transient;
using ArtCommissions.Common.DTOs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace ArtCommissions.BLL
{
    public static class Startup
    {
        public static void RegisterBLLServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            StripeConfiguration.ApiKey = configuration.GetSection("StripeSettings:SecretKey").Get<string>();

            services.AddTransient<IEmailSender, EmailService>();
            services.AddTransient<IStripeService, StripeService>();

            services.AddScoped<IServicesManager, ServicesManager>();

            services.AddSingleton<ILoggerService, LoggerService>();

            EventsSubcriberManager.RegisterSubscribers();
        }
    }
}
