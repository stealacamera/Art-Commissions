using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class InvoicesController : BaseController
    {
        public InvoicesController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesPaginatedListPartial(int orderId, int page = 1)
        {
            var order = await _servicesManager.OrdersService.GetByIdAsync(orderId);
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(order.Commission.Id);
            var currentUserId = GetCurrentUserId();

            if (currentUserId != order.Client.Id && currentUserId != commission.Owner.Id)
                return Unauthorized();
            
            var result = await _servicesManager.InvoicesService.GetAllForOrderAsync(orderId, page, _paginationSize);
            return PartialView("_InvoicesPaginatedListPartial", result);
        }

        [HttpPost("invoices/{id:int}/hasChanged")]
        public async Task<bool> HasInvoiceChanged(int id, Invoice invoice)
        {
            var currentInvoice = await _servicesManager.InvoicesService.GetByIdAsync(id);
            return invoice.Price != currentInvoice.Price || invoice.Status != currentInvoice.Status;
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(InvoiceUpsertRequestModel model)
        {
            var standardExecution = async () =>
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (IActionResult)PartialView("_InvoiceUpsertFormPartial", model);
                }

                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    if (model.Id == 0)
                    {
                        await _servicesManager.OrdersService.SetAsWaitingPaymentAsync(model.OrderId, GetCurrentUserId());
                        await _servicesManager.InvoicesService.AddAsync(model, GetCurrentUserId());
                    }
                    else
                        await _servicesManager.InvoicesService.UpdateAsync(model, GetCurrentUserId());

                    return Ok();
                });
            };

            var onError = async (Exception exception) =>
            {
                return await Task.Run(() =>
                {
                    if (exception is UnauthorizedException)
                        return (IActionResult)Unauthorized();
                    else
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                });
            };

            return await TryExecuteAsync(standardExecution, onError);
        }

        [HttpPatch("orders/{orderId:int}/invoices/{invoiceId:int}/cancel")]
        public async Task<IActionResult> Cancel(int orderId, int invoiceId)
        {
            var standardExecution = async () =>
            {
                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    await _servicesManager.InvoicesService.CancelAsync(invoiceId, orderId, GetCurrentUserId());
                    await _servicesManager.OrdersService.SetInProgressAsync(orderId, GetCurrentUserId());

                    return (IActionResult)Ok();
                });
            };

            var onError = async (Exception exception) =>
            {
                return await Task.Run(() =>
                {
                    if (exception is UnauthorizedException)
                        return (IActionResult)Unauthorized();
                    else if (exception is EmptyInvoiceHistoryException)
                        return BadRequest(exception.Message);
                    else
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                });
            };

            return await TryExecuteAsync(standardExecution, onError);
        }        
    }
}
