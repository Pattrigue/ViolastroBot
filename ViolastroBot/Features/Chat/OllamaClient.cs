using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Features.Chat;

public sealed record ChatTurn(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

public sealed record ChatCompletion(string Text, bool WasTruncated);

public sealed class OllamaClient(IOptions<ChatSettings> options) : ISingleton
{
    private const int ContextSize = 4096;
    private const int KeepLoadedForever = -1;
    private const string LengthLimitReason = "length";

    private readonly ChatSettings _settings = options.Value;

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(options.Value.OllamaUrl),
        Timeout = Timeout.InfiniteTimeSpan,
    };

    public async Task<ChatCompletion> CompleteAsync(
        IReadOnlyList<ChatTurn> messages,
        int maxTokens,
        CancellationToken cancellationToken
    )
    {
        ChatRequest request = new(
            _settings.Model,
            messages,
            Stream: false,
            Think: false,
            KeepLoadedForever,
            new ChatOptions(0.95, 0.9, 1.1, maxTokens, ContextSize, _settings.ThreadCount)
        );

        using var response = await _http.PostAsJsonAsync("/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body =
            await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Ollama returned an empty response.");

        return new ChatCompletion(body.Message.Content, body.DoneReason == LengthLimitReason);
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatTurn> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("think")] bool Think,
        [property: JsonPropertyName("keep_alive")] int KeepAlive,
        [property: JsonPropertyName("options")] ChatOptions Options
    );

    private sealed record ChatOptions(
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("top_p")] double TopP,
        [property: JsonPropertyName("repeat_penalty")] double RepeatPenalty,
        [property: JsonPropertyName("num_predict")] int MaxTokens,
        [property: JsonPropertyName("num_ctx")] int ContextSize,
        [property: JsonPropertyName("num_thread"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            int? ThreadCount
    );

    private sealed record ChatResponse(
        [property: JsonPropertyName("message")] ChatTurn Message,
        [property: JsonPropertyName("done_reason")] string? DoneReason
    );
}
