using Microsoft.Extensions.Hosting;

namespace ArtCommissions.BLL.Services.Singleton;

public interface ILoggerService
{
    Task LogError(string error);
    Task LogError(Exception exception);
    Task LogDebug(string message);
}

internal class LoggerService : ILoggerService
{
    private readonly string _logsFolderPath;

    public LoggerService(IHostEnvironment hostEnv)
    {
        _logsFolderPath = hostEnv.ContentRootPath + "Logs";
        Directory.CreateDirectory(_logsFolderPath);
    }

    private async Task WriteToFile(string fileName, string message)
    {
        fileName = $"{fileName}_{DateTime.Now:dd-MM-yyyy}";
        message = $"{Environment.NewLine}" +
                  $"{DateTime.Now:F}" +
                  $"{Environment.NewLine}" +
                  $"\t{message}" +
                  $"{Environment.NewLine}{Environment.NewLine}";

        await File.AppendAllTextAsync(Path.Combine(_logsFolderPath, fileName), message);
    }

    public Task LogDebug(string message)
    {
        WriteToFile("Debug", message);
        return Task.CompletedTask;
    }

    public Task LogError(string error)
    {
        WriteToFile("Errors", error);
        return Task.CompletedTask;
    }

    public Task LogError(Exception exception)
    {
        LogError(exception.ToString());
        return Task.CompletedTask;
    }
}
