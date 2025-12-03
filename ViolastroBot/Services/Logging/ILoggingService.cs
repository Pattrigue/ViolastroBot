namespace ViolastroBot.Services.Logging;

public interface ILoggingService
{
    Task LogMessageAsync(string message);
}
