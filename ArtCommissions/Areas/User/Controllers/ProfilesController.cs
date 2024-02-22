using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs.ViewModels;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.User.Controllers
{
    [Area("User")]
    public class ProfilesController : BaseController
    {
        public ProfilesController(IServicesManager servicesManager, IConfiguration configuration) : base(servicesManager, configuration) { }

        public async Task<IActionResult> Index(int id, int commissionsPage = 1)
        {
            if(await _servicesManager.AppUsersService.IsInRole(id, "Admin"))
                return new UnauthorizedResult();

            var commissions = await _servicesManager.CommissionsService.GetAllByUserAsync(id, commissionsPage, _paginationSize, excludeClosed: true);

            if (await _servicesManager.AppUsersService.IsLockedOut(id))
                throw new ArtCommissionsException();

            foreach (var commission in commissions.Items)
                commission.SampleImages = await _servicesManager.CommissionSampleImagesService.GetAllForCommissionAsync(commission.Id);

            var profile = new UserProfileVM
            {
                User = await _servicesManager.AppUsersService.GetByIdAsync(id),
                Commissions = commissions,
                OverallReviewScore = await _servicesManager.ReviewsService.GetOverallScoreForUserAsync(id)
            };

            return View(profile);
        }
    }
}
