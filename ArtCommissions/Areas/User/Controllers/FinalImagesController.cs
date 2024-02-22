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
    public class FinalImagesController : BaseController
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly string _finalImagesPath = @"images\orders\final-images";

        public FinalImagesController(
            IWebHostEnvironment hostEnv, 
            IConfiguration configuration, 
            IServicesManager servicesManager) : base(servicesManager, configuration)
        {
            _hostEnv = hostEnv;
        }

        [HttpPost]
        public async Task<IActionResult> Create(FinalImageAddRequestModel model)
        {
            string newFileName = null;

            var standardExecution = async () =>
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (IActionResult)PartialView("_FinalImageAddFormPartial", model);
                }

                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    newFileName = SaveFileToRootPath(_hostEnv.WebRootPath, _finalImagesPath, model.FinalImage);

                    await _servicesManager.FinalImagesService.RemoveForOrderAsync(GetCurrentUserId(), model.OrderId);
                    await _servicesManager.FinalImagesService.AddAsync(GetCurrentUserId(), model.OrderId, newFileName);

                    return Created(string.Empty, null);
                });
            };

            var onError = async (Exception exception) =>
            {
                if (newFileName != null)
                    RemoveFileFromRootPath(_hostEnv.WebRootPath, _finalImagesPath, newFileName);

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
    }
}
