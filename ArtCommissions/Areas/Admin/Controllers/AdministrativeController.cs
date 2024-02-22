using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdministrativeController : BaseController
    {
        public AdministrativeController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(AppUserAddRequestModel model)
        {
            var standardExecution = async () =>
            {
                if(!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (IActionResult)View(model);
                }

                model.Role = "Admin";
                model.StripeAccountId = "0";
                model.StripeCustomerId = "0";

                await _servicesManager.AppUsersService.AddAdminstrativeUserAsync(GetCurrentUserId(), model);
                TempData["Created"] = $"The administrator \"{model.Username}\" was created successfully";
                return View(new AppUserAddRequestModel());
            };

            var onError = async (Exception exception) =>
            {
                return await Task.Run(() =>
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                    return (IActionResult)View(model);
                });
            };

            return await TryExecuteAsync(standardExecution, onError);
        }
    }
}
