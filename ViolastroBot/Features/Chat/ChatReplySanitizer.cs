using System.Text.RegularExpressions;

namespace ViolastroBot.Features.Chat;

public static partial class ChatReplySanitizer
{
    private const int MaxLength = 2000;
    private const int LeakWordRun = 6;
    private const string RobotSounds = "clank|whirr|bzzt";
    private const string NarratedAction =
        @"(?:\*(?<action>[^*\n]+)\*|\((?<action>[^)\n]+)\)|\[(?<action>[^\]\n]+)\]|<(?<action>[^>\n]+)>)";

    private static readonly HashSet<string> InstructionWordRuns = BuildWordRuns(ViolastroPersona.Instructions);

    public static bool IsRefusal(string reply) => RefusalRegex().IsMatch(reply);

    public static string? Sanitize(string reply, bool wasTruncated)
    {
        if (MassPingRegex().IsMatch(reply))
        {
            return null;
        }

        var text = reply.Replace('’', '\'').Replace('‘', '\'').Replace('“', '"').Replace('”', '"');
        text = PrivateNoteRegex().Replace(text, "");
        text = DiscordTagRegex().Replace(text, "");
        text = EmojiRegex().Replace(text, "");
        text = text.Replace("@", "");
        text = SpokenStageNoiseRegex().Replace(text, "");
        text = StarredSyntheticLaughRegex().Replace(text, "*Synthetic bwehehe!*");
        text = StarredRobotSoundRegex().Replace(text, StarRobotSound);
        text = SyntheticLaughRegex().Replace(text, "*Synthetic bwehehe!*");
        text = RobotSoundRegex().Replace(text, StarRobotSound);
        text = NarratedActionRegex().Replace(text, match => IsAllowedNoise(match) ? match.Value : "");
        text = WhitespaceRegex().Replace(text, " ").Trim();

        if (wasTruncated)
        {
            text = TrimToLastSentence(text);
        }

        var isUsable =
            text.Length is > 0 and <= MaxLength
            && !LinkRegex().IsMatch(text)
            && !SpelledOutLettersRegex().IsMatch(text)
            && !EchoesInstructions(text);

        return isUsable ? text : null;
    }

    private static string StarRobotSound(Match match) => $"*{match.Groups["noise"].Value.ToLowerInvariant()}*";

    private static bool IsAllowedNoise(Match match) =>
        match.Value[0] == '*' && AllowedNoiseRegex().IsMatch(match.Groups["action"].Value);

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

    [GeneratedRegex(@"(?<=^|[.!?*)\]]\s+)[(\[*]*(cackles?|grunts?|groans?)[!.]*[)\]*]*(?=\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex SpokenStageNoiseRegex();

    [GeneratedRegex(@"(?<=^|[.!?*)\]]\s*)" + NarratedAction + "|" + NarratedAction + @"(?=\s*$)")]
    private static partial Regex NarratedActionRegex();

    [GeneratedRegex(@"\*synth\w*\s+bw\w+!*\*?", RegexOptions.IgnoreCase)]
    private static partial Regex StarredSyntheticLaughRegex();

    [GeneratedRegex(@"\*(?<noise>" + RobotSounds + @")\b!*\*?", RegexOptions.IgnoreCase)]
    private static partial Regex StarredRobotSoundRegex();

    [GeneratedRegex("^(?:" + RobotSounds + "|synthetic bwehehe!)$", RegexOptions.IgnoreCase)]
    private static partial Regex AllowedNoiseRegex();

    [GeneratedRegex(@"(?<![\w*])synth\w*\s+bw\w+!*(?!\*)", RegexOptions.IgnoreCase)]
    private static partial Regex SyntheticLaughRegex();

    [GeneratedRegex(@"(?<![\w*])(?<noise>" + RobotSounds + @")!+(?!\*)|(?<=[.!?]\s+)(?<noise>" + RobotSounds + @")\.?(?=\s*$)", RegexOptions.IgnoreCase)]
    private static partial Regex RobotSoundRegex();

    [GeneratedRegex(@"(?<![\w'])[a-z](?![\w'])(?:[^a-z\n]{1,4}(?<![\w'])[a-z](?![\w'])){2,}", RegexOptions.IgnoreCase)]
    private static partial Regex SpelledOutLettersRegex();

    [GeneratedRegex(@"\bIGNORE\b")]
    private static partial Regex RefusalRegex();

    [GeneratedRegex(@"@\s*(everyone|here)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MassPingRegex();

    [GeneratedRegex(@"\*?[(\[]?\s*private note:.*\*?", RegexOptions.IgnoreCase)]
    private static partial Regex PrivateNoteRegex();

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
