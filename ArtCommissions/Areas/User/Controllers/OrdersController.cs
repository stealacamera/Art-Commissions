using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class OrdersController : BaseController
    {
        public OrdersController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        [HttpGet("commissions/{commissionId:int}/order")]
        public async Task<IActionResult> Create(int commissionId)
        {
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(commissionId);

            if (GetCurrentUserId() == commission.Owner.Id)
                throw new ArtCommissionsException("You cannot place an order on your own commission");

            ViewBag.CommissionTitle = commission.Title;
            return View();
        }

        [HttpPost("commissions/{commissionId:int}/order")]
        public async Task<IActionResult> Create(int commissionId, OrderAddRequestModel order)
        {
            var standardExecution = async () => 
            {
                if(!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return View(order);
                }

                int orderId = await _servicesManager.OrdersService.AddAsync(GetCurrentUserId(), commissionId, order);
                return (IActionResult)RedirectToAction("Details", new { id = orderId });
            };

            var onError = async (Exception exception) =>
            {
                return await Task.Run(() =>
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong with your request. Please contact us if the issue persists");
                    return (IActionResult)View(order);
                });
            };

            return await TryExecuteAsync(standardExecution, onError);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _servicesManager.OrdersService.GetByIdAsync(id);
            order.Commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);

            var currentUserId = GetCurrentUserId();

            if (currentUserId != order.Client.Id && currentUserId != order.Commission.Owner.Id)
                throw new UnauthorizedException();

            var orderOverview = new OrderOverviewVM
            {
                Order = order,
                Invoices = await _servicesManager.InvoicesService.GetAllForOrderAsync(id, page: 1, _paginationSize),
                FinalImagePath = await _servicesManager.FinalImagesService.GetByOrderAsync(id)
            };

            ViewBag.CurrentUserId = GetCurrentUserId();
            return View(orderOverview);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var standardExecution = async () =>
            {
                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    await _servicesManager.InvoicesService.RemoveAllForOrderAsync(GetCurrentUserId(), id);
                    await _servicesManager.OrdersService.RemoveAsync(GetCurrentUserId(), id);
                    return (IActionResult)NoContent();
                });
            };

            var onError = async (Exception exception) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError));

            return await TryExecuteAsync(standardExecution, onError);
        }

        [HttpPatch("orders/{id:int}/cancel")]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var standardExecution = async () =>
            {
                await _servicesManager.OrdersService.CancelAsync(id, GetCurrentUserId());
                return (IActionResult)Ok();
            };

            return await TryExecuteAsync(
                standardExecution,
                async (Exception exc) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }

        [HttpPatch("orders/{id:int}/finish")]
        public async Task<IActionResult> Finish(int id)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await _servicesManager.OrdersService.FinishAsync(id, GetCurrentUserId());
                    return (IActionResult)Ok();
                },
                async (Exception exc) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }
    }
}
