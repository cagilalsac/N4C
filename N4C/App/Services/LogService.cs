using Microsoft.Extensions.Logging;

namespace N4C.App.Services
{
    public class LogService : Service
    {
        protected ILogger<LogService> Logger { get; }

        public LogService(ILogger<LogService> logger)
        {
            Logger = logger;
        }

        public void LogError(string message)
        {
            Logger.LogError(message);
        }
    }
}
