namespace ViolastroBot.DiscordServerConfiguration;

public sealed class ChatSettings
{
    public string OllamaUrl { get; init; } = "http://127.0.0.1:11434";
    public string Model { get; init; } = "gemma4:e4b";
    public int? ThreadCount { get; init; }
}
