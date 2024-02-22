using ArtCommissions.BLL.Services;
using ArtCommissions.BLL.Services.Hosted;

namespace ArtCommissions.BLL
{
    internal static class EventsSubcriberManager
    {
        public static void RegisterSubscribers()
        {
            CommissionsService.AdminRemovedCommission += EmailService.OnAdminRemovedCommission;

            AppUsersService.UserLockedOut += EmailService.OnUserLockedOut;

            OrdersService.OrderConcludedByClient += EmailService.OnOrderConcludedByClient;
            OrdersService.OrderRejectedByVendor += EmailService.OnOrderRejectedByVendor;

            InvoicesService.InvoiceAdded += EmailService.OnInvoiceAdded;
            InvoicesService.InvoiceUpdated += EmailService.OnInvoiceUpdated;
            InvoicesService.InvoicePaid += EmailService.OnInvoicePayed;
        }
    }
}
