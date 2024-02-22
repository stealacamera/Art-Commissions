using ArtCommissions.BLL;
using ArtCommissions.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtCommissions.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IServicesManager _servicesManager;
        protected readonly int _paginationSize;

        public BaseController(IServicesManager servicesManager, IConfiguration configuration)
        {
            if(!int.TryParse(configuration["AppSettings:PageSize"], out _paginationSize))
                throw new ArtCommissionsException("\"AppSettings:PageSize\" configuration key missing");
            
            _servicesManager = servicesManager;
        }

        protected int GetCurrentUserId()
        {
            return User.Identity.IsAuthenticated ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : 0;
        }

        protected string EllipseString(string text, int maxLength)
        {
            return text.AsSpan(0, maxLength).ToString() + "...";
        }

        protected async Task<T> TryExecuteAsync<T>(Func<Task<T>> standardExecution, Func<Exception, Task<T>> exceptionHandler) where T : class
        {
            T result = null;

            try
            {
                result = await standardExecution();
            }
            catch (ArtCommissionsException exc)
            {
                // TODO
                result = await exceptionHandler(exc);
            }
            catch (Exception exc)
            {
                // TODO show to user

                _servicesManager.LoggerService.LogError(exc);
                result = await exceptionHandler(exc);
            }

            return result;
        }

        protected string SaveFileToRootPath(string rootPath, string folderPath, IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString();

            var uploadPath = Path.Combine(rootPath, folderPath);
            var extension = Path.GetExtension(file.FileName);

            Directory.CreateDirectory(uploadPath);
            using (var fileStr = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
            {
                file.CopyTo(fileStr);
            }

            return fileName + extension;
        }

        protected void RemoveFileFromRootPath(string rootPath, string folderPath, string fileName) 
        {
            string path = Path.Combine(rootPath, folderPath, fileName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}