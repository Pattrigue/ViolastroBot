using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Features.Chat;

public sealed partial class ChatReplyWordFilter : ISingleton
{
    [method: JsonConstructor]
    private sealed class BlockedWords(string[] nWords, char[] nVariants, string[]? otherWords)
    {
        public string[] NWords { get; } = nWords;
        public char[] NVariants { get; } = nVariants;
        public string[] OtherWords { get; } = otherWords ?? [];
    }

    private const ulong BlockedWordsMessageId = 1148574093948489810;

    private readonly DiscordSocketClient _client;

    private BlockedWords? _blockedWords;

    public ChatReplyWordFilter(DiscordSocketClient client)
    {
        _client = client;
        _client.Ready += FetchBlockedWordsAsync;
    }

    public bool IsSafeToSend(string content)
    {
        return _blockedWords != null
            && !ContainsBlockedWord(content, _blockedWords.NWords, _blockedWords.NVariants, _blockedWords.OtherWords);
    }

    public static bool ContainsBlockedWord(string content, string[] stems, char[] stemPrefixes, string[] otherWords)
    {
        var letters = NonLetterRegex().Replace(content.ToLowerInvariant(), "");
        var prefixes = stemPrefixes.Select(char.ToLowerInvariant).ToHashSet();

        return otherWords.Any(word => word.Length > 0 && letters.Contains(word.ToLowerInvariant()))
            || stems.Any(stem => stem.Length > 0 && HasPrefixedStem(letters, stem.ToLowerInvariant(), prefixes));
    }

    private static bool HasPrefixedStem(string letters, string stem, HashSet<char> prefixes)
    {
        for (
            var index = letters.IndexOf(stem, StringComparison.Ordinal);
            index >= 0;
            index = letters.IndexOf(stem, index + 1, StringComparison.Ordinal)
        )
        {
            if (index > 0 && prefixes.Contains(letters[index - 1]))
            {
                return true;
            }
        }

        return false;
    }

    private async Task FetchBlockedWordsAsync()
    {
        if (_client.GetChannel(Channels.OffensiveWords) is not SocketTextChannel channel)
        {
            Console.WriteLine("Failed to fetch the blocked words channel.");
            return;
        }

        if (await channel.GetMessageAsync(BlockedWordsMessageId) is not IUserMessage message)
        {
            Console.WriteLine("Failed to fetch the blocked words message.");
            return;
        }

        try
        {
            _blockedWords = JsonConvert.DeserializeObject<BlockedWords>(message.Content);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse the blocked words message: {ex.Message}");
        }
    }

    [GeneratedRegex(@"\P{L}")]
    private static partial Regex NonLetterRegex();
}
