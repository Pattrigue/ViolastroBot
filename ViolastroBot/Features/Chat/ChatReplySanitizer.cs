using System.Text.RegularExpressions;

namespace ViolastroBot.Features.Chat;

public static partial class ChatReplySanitizer
{
    private const int MaxLength = 2000;
    private const int LeakWordRun = 6;

    private static readonly HashSet<string> InstructionWordRuns = BuildWordRuns(ViolastroPersona.Instructions);

    public static string? Sanitize(string reply, bool wasTruncated)
    {
        var text = reply.Replace('’', '\'').Replace('‘', '\'').Replace('“', '"').Replace('”', '"');
        text = DiscordTagRegex().Replace(text, "");
        text = EmojiRegex().Replace(text, "");
        text = text.Replace("@", "");
        text = WhitespaceRegex().Replace(text, " ").Trim();

        if (wasTruncated)
        {
            text = TrimToLastSentence(text);
        }

        var isUsable = text.Length is > 0 and <= MaxLength && !LinkRegex().IsMatch(text) && !EchoesInstructions(text);

        return isUsable ? text : null;
    }

    private static string TrimToLastSentence(string text)
    {
        var lastPunctuation = text.LastIndexOfAny(['!', '?', '.']);

        if (lastPunctuation < 0)
        {
            return "";
        }

        var end = lastPunctuation + 1;

        if (end < text.Length && text[end] == '*')
        {
            end++;
        }

        return text[..end];
    }

    private static bool EchoesInstructions(string text)
    {
        return WordRuns(text).Any(InstructionWordRuns.Contains);
    }

    private static HashSet<string> BuildWordRuns(string text)
    {
        return WordRuns(text).ToHashSet();
    }

    private static IEnumerable<string> WordRuns(string text)
    {
        var words = WordRegex().Matches(text.ToLowerInvariant()).Select(match => match.Value).ToArray();

        for (var i = 0; i <= words.Length - LeakWordRun; i++)
        {
            yield return string.Join(' ', words, i, LeakWordRun);
        }
    }

    [GeneratedRegex(@"<(@[!&]?|#)\d+>")]
    private static partial Regex DiscordTagRegex();

    [GeneratedRegex(@"[\p{So}\p{Cs}️‍]")]
    private static partial Regex EmojiRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"https?://|www\.|discord\.gg|discord\.com/invite", RegexOptions.IgnoreCase)]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"[a-z']+")]
    private static partial Regex WordRegex();
}
