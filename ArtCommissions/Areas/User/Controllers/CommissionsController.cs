using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtCommissions.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]
    public class CommissionsController : BaseController
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly string _sampleImagesPath = @"images\commissions\sample-images";

        public CommissionsController(
            IWebHostEnvironment hostEnv,
            IConfiguration configuration,
            IServicesManager servicesManager) : base(servicesManager, configuration)
        {
            _hostEnv = hostEnv;
        }

        private async Task<List<SelectListItem>> GetTagsSelectList()
        {
            var tags = await _servicesManager.TagsService.GetAllAsync();
            var selectListTags = new List<SelectListItem>();

            foreach (var tag in tags)
                selectListTags.Add(new SelectListItem { Value = tag.Id.ToString(), Text = tag.Name });

            return selectListTags;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await GetTagsSelectList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommissionAddRequestModel model)
        {
            List<string> newFilesNames = new List<string>();

            var standardExecution = async () =>
            {
                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    var currentUserId = GetCurrentUserId();
                    int commissionId = await _servicesManager.CommissionsService
                                                             .AddAsync(
                                                                 currentUserId,
                                                                 new Commission
                                                                 {
                                                                     Description = model.Description,
                                                                     IsClosed = model.IsClosed,
                                                                     MinPrice = model.MinPrice,
                                                                     Title = model.Title
                                                                 });

                    foreach (var tag in model.Tags)
                        await _servicesManager.CommissionTagsService.AddAsync(currentUserId, commissionId, tag);

                    foreach (var image in model.SampleImages)
                    {
                        string newFileName = SaveFileToRootPath(_hostEnv.WebRootPath, _sampleImagesPath, image);
                        newFilesNames.Add(newFileName);

                        await _servicesManager.CommissionSampleImagesService.AddAsync(commissionId, newFileName);
                    }

                    return RedirectToAction("Index", "Dashboards");
                });
            };

            var onError = async (Exception exception) =>
            {
                foreach (var fileName in newFilesNames)
                    RemoveFileFromRootPath(_hostEnv.WebRootPath, _sampleImagesPath, fileName);

                ModelState.AddModelError(string.Empty, "Something went wrong and your request couldn't be processed. If this problem persists, please contact us");
                return await Task.Run(() => (IActionResult)View(model));
            };

            return await TryExecuteAsync(
                async () => ModelState.IsValid ? await standardExecution() : View(model),
                onError);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(id);

            if (GetCurrentUserId() != commission.Owner.Id)
                throw new UnauthorizedException();

            var existingSampleImages = new List<CommissionSampleImageUpdateRequestModel>();

            foreach (var image in await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(id))
                existingSampleImages.Add(new CommissionSampleImageUpdateRequestModel { Id = image.Id, ImageName = image.ImageName, ShouldRemove = false });

            CommissionEditRequestModel model = new CommissionEditRequestModel
            {
                Title = commission.Title,
                Description = commission.Description,
                IsClosed = commission.IsClosed,
                MinPrice = commission.MinPrice,
                ExistingSampleImages = existingSampleImages,
                Tags = (await _servicesManager.CommissionTagsService.GetAllForCommissionAsync(id)).Select(e => e.Id).ToList()
            };

            ViewBag.Tags = await GetTagsSelectList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CommissionEditRequestModel model)
        {
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(id);
            List<string> newFilesNames = new List<string>(), filesNamesRemoved = new List<string>();

            var standardExecution = async () =>
            {
                if (model.ExistingSampleImages.All(e => e.ShouldRemove == true) && model.SampleImages == null)
                    ModelState.AddModelError(string.Empty, "You have to have at least one sample image on your commission");

                if (!ModelState.IsValid)
                {
                    ViewBag.Tags = await GetTagsSelectList();
                    Response.StatusCode = StatusCodes.Status400BadRequest;

                    return (IActionResult)View(model);
                }

                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    await _servicesManager.CommissionsService
                                          .UpdateAsync(
                                            GetCurrentUserId(),
                                            new Commission
                                            {
                                                Description = model.Description,
                                                Id = id,
                                                IsClosed = model.IsClosed,
                                                MinPrice = model.MinPrice,
                                                Title = model.Title
                                            });

                    // Update tags
                    var currentTags = await _servicesManager.CommissionTagsService.GetAllForCommissionAsync(id);

                    foreach (var tag in currentTags)
                    {
                        if (!model.Tags.Contains(tag.Id))
                            await _servicesManager.CommissionTagsService.RemoveAsync(GetCurrentUserId(), id, tag.Id);
                    }

                    foreach (var tag in model.Tags)
                    {
                        if (currentTags.FirstOrDefault(e => e.Id == tag) == null)
                            await _servicesManager.CommissionTagsService.AddAsync(GetCurrentUserId(), id, tag);
                    }

                    // Add new images
                    if (model.SampleImages != null)
                    {
                        foreach (var image in model.SampleImages)
                        {
                            string newFileName = SaveFileToRootPath(_hostEnv.WebRootPath, _sampleImagesPath, image);
                            newFilesNames.Add(newFileName);

                            await _servicesManager.CommissionSampleImagesService.AddAsync(id, newFileName);
                        }
                    }

                    // Update existing images
                    foreach (var existingImage in model.ExistingSampleImages)
                    {
                        if (existingImage.ShouldRemove)
                        {
                            await _servicesManager.CommissionSampleImagesService.RemoveAsync(existingImage.Id);
                            filesNamesRemoved.Add(existingImage.ImageName);
                        }
                    }

                    filesNamesRemoved.ForEach(file => RemoveFileFromRootPath(_hostEnv.ContentRootPath, _sampleImagesPath, file));
                    return RedirectToAction("Details", new { id });
                });
            };

            var onError = async (Exception exception) =>
            {
                ModelState.AddModelError(string.Empty, "Something went wrong with your request, please try again later");
                return await Task.Run(() => (IActionResult)View(model));
            };

            return await TryExecuteAsync(standardExecution, onError);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var commission = await _servicesManager.CommissionsService.GetByIdAsync(id);

            if (await _servicesManager.AppUsersService.IsLockedOut(commission.Owner.Id))
                throw new ArtCommissionsException();

            commission.Owner = await _servicesManager.AppUsersService.GetByIdAsync(commission.Owner.Id);
            commission.SampleImages = await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(id);

            CommissionOverviewVM model = new CommissionOverviewVM
            {
                Commission = commission,
                Reviews = await _servicesManager.ReviewsService.GetAllForCommissionAsync(id, 1, _paginationSize),
                OverallReviewsScore = await _servicesManager.ReviewsService.GetOverallScoreForCommissionAsync(id),
                Tags = await _servicesManager.CommissionTagsService.GetAllForCommissionAsync(id)
            };

            return View(model);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var standardExecution = async () =>
            {
                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    var currentUserId = GetCurrentUserId();

                    await _servicesManager.CommissionSampleImagesService.RemoveAllByCommissionAsync(currentUserId, id);
                    await _servicesManager.CommissionsService.RemoveAsync(currentUserId, id);

                    return (IActionResult)NoContent();
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

        [HttpPatch]
        public async Task<IActionResult> ChangeStatus(int id, bool newStatus)
        {
            var standardExecution = async () =>
            {
                await _servicesManager.CommissionsService.UpdateStatusAsync(GetCurrentUserId(), id, newStatus);
                return (IActionResult)Ok();
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
    }
}
