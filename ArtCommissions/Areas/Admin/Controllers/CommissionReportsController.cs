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
    public class CommissionReportsController : BaseReportsController
    {
        public CommissionReportsController(IConfiguration configuration, IServicesManager servicesManager) : base(configuration, servicesManager) { }

        public async Task<IActionResult> Index(ReportStatus status = ReportStatus.PENDING, int page = 1)
        {
            var reports = await _servicesManager.CommissionReportsService.GetAllByStatusAsync(status, page, _paginationSize);

            foreach (var report in reports.Items)
            {
                if (report.Reason.Length > _reasonMaxLength)
                    report.Reason = EllipseString(report.Reason, _reasonMaxLength);

                report.ReportedCommission = await _servicesManager.CommissionsService.GetByIdAsync(report.ReportedCommission.Id);
            }

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _servicesManager.CommissionReportsService.GetByIdAsync(id);
            report.ReportedCommission = await _servicesManager.CommissionsService.GetByIdAsync(report.ReportedCommission.Id);

            return View(report);
        }

        [AllowAnonymous]
        [HttpPost("commissions/{id:int}/report")]
        public async Task<IActionResult> Create(int id, CommissionReportAddRequestModel model)
        {
            var standardExecution = async () =>
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return PartialView("/Areas/User/Views/Shared/_CommissionReportAddFormPartial.cshtml", model);
                }

                model.ReportedCommissionId = id;
                await _servicesManager.CommissionReportsService.AddAsync(GetCurrentUserId(), model);

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
                    var report = await _servicesManager.CommissionReportsService.GetByIdAsync(id);
                    var commission = await _servicesManager.CommissionsService.GetByIdAsync(report.ReportedCommission.Id);

                    await _servicesManager.CommissionReportsService.UpdateStatusAsync(id, ReportStatus.ACCEPTED, GetCurrentUserId());

                    // Remove all pending reports about this commission
                    await _servicesManager.CommissionReportsService.RemoveAllPendingForCommissionAsync(GetCurrentUserId(), report.ReportedCommission.Id);

                    // Add strike to user
                    await _servicesManager.AppUsersService.AddSuspensionStrikeAsync(_maxNrSuspensionStrikes, GetCurrentUserId(), commission.Owner.Id);

                    // Take down commission                    
                    await _servicesManager.CommissionSampleImagesService.RemoveAllByCommissionAsync(GetCurrentUserId(), report.ReportedCommission.Id);
                    await _servicesManager.CommissionsService.RemoveAsync(GetCurrentUserId(), report.ReportedCommission.Id);

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
                    await _servicesManager.CommissionReportsService.UpdateStatusAsync(id, ReportStatus.REJECTED, GetCurrentUserId());
                    return (IActionResult)Ok();
                },
                async (Exception) => await Task.Run(() => (IActionResult)new StatusCodeResult(StatusCodes.Status500InternalServerError)));
        }
    }
}
