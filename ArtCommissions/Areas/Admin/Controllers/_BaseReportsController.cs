using ArtCommissions.BLL;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Areas.Admin.Controllers
{
    public abstract class BaseReportsController : BaseController
    {
        protected readonly int _reasonMaxLength = 300;
        protected readonly int _maxNrSuspensionStrikes;

        public BaseReportsController(IConfiguration configuration, IServicesManager servicesManager) : base(servicesManager, configuration)
        {
            if (!int.TryParse(configuration["AppSettings:MaxNrSuspensionStrikes"], out _maxNrSuspensionStrikes))
            {
                _maxNrSuspensionStrikes = 5;
            }
        }

        public abstract Task<IActionResult> AcceptReport(int id);
        public abstract Task<IActionResult> RejectReport(int id);
    }
}
