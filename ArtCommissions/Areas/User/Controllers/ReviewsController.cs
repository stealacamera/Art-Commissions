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
    public class ReviewsController : BaseController 
    {
        public ReviewsController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        [HttpGet]
        public async Task<IActionResult> GetReviewsPaginatedListPartial(int commissionId, int page)
        {
            var result = await _servicesManager.ReviewsService.GetAllForCommissionAsync(commissionId, page, _paginationSize);
            return PartialView("_ReviewsPaginatedListPartial", result);
        }

        [HttpPost("commissions/{id:int}/reviews")]
        public async Task<IActionResult> Create(int id, ReviewAddRequestModel model)
        {
            var standardExecution = async () =>
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (IActionResult)PartialView("_ReviewAddFormPartial", model);
                }

                await _servicesManager.ReviewsService.AddAsync(id, GetCurrentUserId(), model);
                return Created(string.Empty, null);
            };

            var onError = async (Exception exception) =>
            {
                return await Task.Run(() =>
                {
                    if (exception is ExistingReviewForCommissionException)
                        return new StatusCodeResult(StatusCodes.Status409Conflict);
                    else
                        return (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError);
                });
            };

            return await TryExecuteAsync(standardExecution, onError);
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int commissionId)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await _servicesManager.ReviewsService.RemoveAsync(GetCurrentUserId(), commissionId);
                    return (IActionResult)NoContent();
                },
                async (Exception) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }
    }
}
