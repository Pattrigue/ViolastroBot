namespace ViolastroBot.Features;

public interface IStartupTask : IActivateOnStartup
{
    Task InitializeAsync();
}
