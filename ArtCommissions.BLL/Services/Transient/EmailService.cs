using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Enums;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ArtCommissions.BLL.Services.Hosted;

internal class EmailService : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(MailboxAddress.Parse("info@artcommissions.com"));
        emailMessage.To.Add(MailboxAddress.Parse(email));

        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        using (var emailClient = new SmtpClient())
        {
            emailClient.Connect(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            emailClient.Authenticate(_emailSettings.InfoDeskEmail, _emailSettings.InfoDeskPassword);
            emailClient.Send(emailMessage);
            emailClient.Disconnect(true);
        }

        return Task.CompletedTask;
    }

    public static async Task OnUserLockedOut(IServiceProvider serviceProvider, AppUser user)
    {
        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());

        await emailService.SendEmailAsync(
            user.Email,
            "Account locked out for TOS violation",
            $"Your account with the username \"{user.Username}\" was locked down for violating TOS." +
             "You will not be able to use your account anymore.");
    }

    public static async Task OnAdminRemovedCommission(IServiceProvider serviceProvider, AppUser vendor, Commission commission)
    {
        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());

        await emailService.SendEmailAsync(
            vendor.Email,
            "Commission removed for TOS violation",
            $"Your \"{commission.Title}\" commission was removed for violating the websites TOS.\n" +
             "This has added a strike to your account. If you keep getting strikes, your account will be locked down.");
    }

    public static async Task OnInvoiceUpdated(IServiceProvider serviceProvider, AppUser client, Order order)
    {
        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());

        await emailService.SendEmailAsync(
            client.Email,
            $"Invoice for \"{order.Title}\" updated",
            $"The invoice for your \"{order.Title}\" order was updated. Go to your dashboard to get more information");
    }

    public static async Task OnInvoiceAdded(IServiceProvider serviceProvider, AppUser client, Order order, bool hasPreviousInvoiceHistory)
    {
        string emailTitle, emailBody;

        if (hasPreviousInvoiceHistory)
        {
            emailTitle = $"New payment request for \"{order.Title}\"";
            emailBody = $"A new payment request was placed for your \"{order.Title}\" order. Go to your dashboard to see the details";
        }
        else
        {
            emailTitle = $"Request \"{order.Title}\" was accepted";
            emailBody = $"Congrats: The artist has accepted your request!\n\nAn invoice was set:\nPrice: {order.TotalPrice}" +
                         "\n\nGet more information by going to your dashboard.";
        }

        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());
        await emailService.SendEmailAsync(client.Email, emailTitle, emailBody);
    }

    public static async Task OnOrderConcludedByClient(IServiceProvider serviceProvider, AppUser vendor, string orderTitle, OrderStatus conclusionStatus)
    {
        string emailTitle, emailBody;

        if (conclusionStatus == OrderStatus.CANCELLED)
        {
            emailTitle = $"\"{orderTitle}\" order was cancelled";
            emailBody = $"The client of your \"{orderTitle}\" cancelled the request.";
        }
        else
        {
            emailTitle = $"Final product of \"{orderTitle}\" order was accepted";
            emailBody = $"The client of your \"{orderTitle}\" order accepted the final product you submitted!";
        }

        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());
        await emailService.SendEmailAsync(vendor.Email, emailTitle, emailBody);
    }

    public static async Task OnOrderRejectedByVendor(IServiceProvider serviceProvider, AppUser client, string orderTitle)
    {
        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());

        await emailService.SendEmailAsync(
            client.Email,
            $"\"{orderTitle}\" request was rejected",
            $"Your \"{orderTitle}\" request was rejected by the artist");
    }

    public static async Task OnInvoicePayed(IServiceProvider serviceProvider, AppUser vendor, Order order)
    {
        var emailService = new EmailService(serviceProvider.GetRequiredService<IOptions<EmailSettings>>());

        await emailService.SendEmailAsync(
            vendor.Email,
            $"Payment for \"{order.Title}\" was completed by client",
            $"The payment for your order \"{order.Title}\" was fully completed by the client. You can continue with your order now!");
    }
}
