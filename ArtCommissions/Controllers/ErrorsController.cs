using ArtCommissions.Common.Exceptions;
using ArtCommissions.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArtCommissions.Controllers
{
    public class ErrorsController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            ErrorViewModel model = new ErrorViewModel();
            
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var error = exceptionHandlerPathFeature.Error;

            switch (error)
            {
                case UnauthorizedException:
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    model.ErrorMessage = error.Message;
                    break;
                case EntityNotFoundException:
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    model.ErrorMessage = "We could not find what you were looking for";
                    break;
                case ExistingUnpaidInvoiceException:
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    model.ErrorMessage = error.Message;
                    break;
                case ArtCommissionsException:
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    model.ErrorMessage = error.Message ?? "Your request could not be processed. Please make sure there are no syntax errors or invalid data in your request.";
                    break;
                default:
                    Response.StatusCode = StatusCodes.Status500InternalServerError;
                    model.ErrorMessage = "Something went wrong. Please try again later.";
                    break;
            }

            return View(model);
        }
    }
}
