namespace ViolastroBot.Logging;

public interface ILoggingService
{
    Task LogMessageAsync(string message);
}
