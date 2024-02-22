using ArtCommissions.BLL;
using ArtCommissions.Common.DTOs;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserReportsController : BaseReportsController 
    {
        public UserReportsController(IConfiguration configuration, IServicesManager servicesManager) : base(configuration, servicesManager) { }

        public async Task<IActionResult> Index(ReportStatus status = ReportStatus.PENDING, int page = 1)
        {
            var reports = await _servicesManager.UserReportsService.GetAllByStatusAsync(status, page, _paginationSize);

            foreach(var report in reports.Items)
            {
                if (report.Reason.Length > _reasonMaxLength)
                    report.Reason = EllipseString(report.Reason, _reasonMaxLength);

                report.ReportedUser = await _servicesManager.AppUsersService.GetByIdAsync(report.ReportedUser.Id);
            }

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _servicesManager.UserReportsService.GetByIdAsync(id);
            report.ReportedUser = await _servicesManager.AppUsersService.GetByIdAsync(report.ReportedUser.Id);

            return View(report);
        }

        [AllowAnonymous]
        [HttpPost("users/{id:int}/report")]
        public async Task<IActionResult> Create(int id, UserReportAddRequestModel model)
        {
            var standardExecution = async () =>
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return PartialView("/Areas/User/Views/Shared/_UserReportAddFormPartial.cshtml", model);
                }

                model.ReportedUserId = id;
                await _servicesManager.UserReportsService.AddAsync(GetCurrentUserId(), model);

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

        [HttpPatch]
        public override async Task<IActionResult> AcceptReport(int id)
        {
            var standardExecution = async () =>
            {
                return await _servicesManager.WrapInTransactionAsync(async () =>
                {
                    var report = await _servicesManager.UserReportsService.GetByIdAsync(id);

                    await _servicesManager.UserReportsService.UpdateStatusAsync(id, ReportStatus.ACCEPTED, GetCurrentUserId());

                    // Remove all pending reports for this user
                    await _servicesManager.UserReportsService.RemoveAllPendingForUserAsync(GetCurrentUserId(), report.ReportedUser.Id);

                    // Lockout user from their account
                    await _servicesManager.AppUsersService.LockoutAsync(_maxNrSuspensionStrikes, GetCurrentUserId(), report.ReportedUser.Id);

                    return (IActionResult)Ok();
                });
            };

            return await TryExecuteAsync(
                standardExecution,
                async (Exception) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }
        
        [HttpPatch]
        public override async Task<IActionResult> RejectReport(int id)
        {
            return await TryExecuteAsync(
                async () =>
                {
                    await _servicesManager.UserReportsService.UpdateStatusAsync(id, ReportStatus.REJECTED, GetCurrentUserId());
                    return (IActionResult)Ok();
                },
                async (Exception) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }
    }
}
