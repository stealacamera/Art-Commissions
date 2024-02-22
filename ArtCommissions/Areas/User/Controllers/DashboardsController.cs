using ArtCommissions.BLL;
using ArtCommissions.Common.Enums;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class DashboardsController : BaseController
    {
        public DashboardsController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        public async Task<IActionResult> Index(int page = 1)
        {    
            int descriptionMaxLength = 150, userId = GetCurrentUserId();
            var userComissions = await _servicesManager.CommissionsService.GetAllByUserAsync(userId, page, _paginationSize);

            foreach (var commission in userComissions.Items)
            {
                if(commission.Description?.Length > descriptionMaxLength)
                    commission.Description = EllipseString(commission.Description, descriptionMaxLength);

                commission.SampleImages = await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(commission.Id);
            }

            return View(userComissions);
        }

        public async Task<IActionResult> IncomingOrders(OrderStatus status = OrderStatus.IN_PROGRESS, int page = 1)
        {
            int descriptionMaxLength = 200;
            var incomingOrders = await _servicesManager.OrdersService.GetAllForVendorAsync(GetCurrentUserId(), status, page, _paginationSize);

            foreach (var order in incomingOrders.Items)
            {
                order.Client = await _servicesManager.AppUsersService.GetByIdAsync(order.Client.Id);
                order.Commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);

                if (order.Description?.Length > descriptionMaxLength)
                    order.Description = EllipseString(order.Description, descriptionMaxLength);
            }

            ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus));
            return View(incomingOrders);
        }

        public async Task<IActionResult> OutgoingOrders(OrderStatus status = OrderStatus.IN_PROGRESS, int page = 1)
        {
            int descriptionMaxLength = 200;
            var outgoingOrders = await _servicesManager.OrdersService.GetAllByClientAsync(GetCurrentUserId(), status, page, _paginationSize);

            foreach (var order in outgoingOrders.Items)
            {
                order.Commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);
                order.Commission.Owner = await _servicesManager.AppUsersService.GetByIdAsync(order.Commission.Owner.Id);

                if (order.Description?.Length > descriptionMaxLength)
                    order.Description = EllipseString(order.Description, descriptionMaxLength);
            }

            ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus));
            return View(outgoingOrders);
        }
    }
}
