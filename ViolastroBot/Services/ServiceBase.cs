namespace ViolastroBot.Services;

public abstract class ServiceBase
{
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
