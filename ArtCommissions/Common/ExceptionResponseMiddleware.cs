using ArtCommissions.BLL.Services.Singleton;

namespace ArtCommissions.Common;

public class ExceptionResponseMiddleware : IMiddleware
{
    private readonly ILoggerService _loggerService;

    public ExceptionResponseMiddleware(ILoggerService loggerService) => _loggerService = loggerService;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            _loggerService.LogDebug($"Request sent to {context.Request.Path.Value}");
            await next(context);
            _loggerService.LogDebug("Request was processed successfully");
        }
        catch (Exception exc)
        {
            _loggerService.LogError(exc);
            throw;
        }        
    }
}
