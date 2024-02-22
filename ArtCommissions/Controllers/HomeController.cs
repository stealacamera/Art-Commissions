using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.DTOs.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtCommissions.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        public async Task<IActionResult> Index(int page = 1)
        {
            var currentUserId = GetCurrentUserId();

            // If user is logged in as an admin, direct to user reports page
            if (currentUserId != 0 && await _servicesManager.AppUsersService.IsInRole(GetCurrentUserId(), "Admin"))
                return RedirectToAction("Index", "UserReports", new { area = "Admin" });

            // Otherwise direct homepage
            var commissions = await _servicesManager.CommissionsService.GetAllAsync(page, _paginationSize);

            foreach (var commission in commissions.Items)
                commission.SampleImages = await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(commission.Id);

            ViewBag.CurrentUserId = currentUserId;
            return View(commissions);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? q = null, int? tagId = null, int page = 1)
        {
            var tags = new List<SelectListItem> { new SelectListItem { Text = "Select a tag", Value = "0" } };

            foreach (var tag in await _servicesManager.TagsService.GetAllAsync())
                tags.Add(new SelectListItem
                {
                    Text = tag.Name,
                    Value = tag.Id.ToString(),
                    Selected = tagId != null && tag.Id == tagId
                });

            ViewBag.Tags = tags;

            if (string.IsNullOrEmpty(q) && (tagId == null || tagId <= 0))
                return View(new PaginatedList<Commission>());

            var commissions = await _servicesManager.CommissionsService.FilterByKeywords(page, _paginationSize, q, tagId);
            return View(commissions);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}