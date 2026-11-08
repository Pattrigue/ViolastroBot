namespace ViolastroBot.DiscordServerConfiguration;

public sealed class ChatSettings
{
    public ulong[] ChannelIds { get; init; } = [];
    public string OllamaUrl { get; init; } = "http://127.0.0.1:11434";
    public string Model { get; init; } = "qwen3:8b";
    public int? ThreadCount { get; init; }
}
