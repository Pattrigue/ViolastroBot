using System.Text.RegularExpressions;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Features.MessageStrategies;

public sealed partial class QuestionsAnswersStrategy(DiscordSocketClient client) : IMessageStrategy
{
    private static readonly List<string> GameSynonyms = ["vv", "vibrant venture", "the game", "vibrantventure"];

    private static readonly List<string> ReleaseAnswers =
    [
        "Ya fool! Even I don't know that!",
        "Nobody knows!",
        "Sooner or later!",
    ];

    private static readonly List<string> FaqAnswers =
    [
        "Bwehehe! Someone hasn't read the <#567008774288703507>!",
        "Y'all best read the <#567008774288703507>!",
        "Y'all should totally check out the <#567008774288703507>!",
    ];

    private static readonly List<string> MultiplayerAnswers =
    [
        "Yes! Bwehehe!",
        "I lied all these years! It will have multiplayer! Bwehehe!",
        "Y'all never saw it comin', but yes!",
    ];

    private static readonly List<string> IsThisTrueAnswers =
    [
        "Y'all better believe it!",
        "Nah, that ain't true!",
        "Ain't no way!",
        "Bwehehe, it seems so!",
        "I have no idea!",
        "Who knows?!!",
        "I ain't got a clue!",
        "There's no telling, I'm telling y'all!",
        "No clue! None!",
        "Bwuh, why you askin' me?",
        "No idea!",
        "Now do y'all expect me to know that?",
    ];

    private static readonly Dictionary<string, List<string>> QuestionsMultipleAnswers = new()
    {
        { "when will vv be released", ["Ya fool! Even I don't know that!", "Nobody knows!", "Sooner or later!"] },
        { "when will vv release", ReleaseAnswers },
        { "when is vv going to be released", ReleaseAnswers },
        { "when is vv going to release", ReleaseAnswers },
        { "when is vv gonna release", ReleaseAnswers },
        { "will vv have multiplayer", MultiplayerAnswers },
        { "will vv be multiplayer", MultiplayerAnswers },
        { "is vv gonna have multiplayer", MultiplayerAnswers },
        { "is vv going to have multiplayer", MultiplayerAnswers },
        { "is vv multiplayer", MultiplayerAnswers },
        { "will vv be free", FaqAnswers },
        { "is vv gonna be free", FaqAnswers },
        { "is vv going to be free", FaqAnswers },
        { "is vv free", FaqAnswers },
        { "will vv cost money", FaqAnswers },
        { "is vv gonna cost money", FaqAnswers },
        { "is vv going to cost money", FaqAnswers },
        { "does vv cost money", FaqAnswers },
        { "is violastro bald", ["What! Are ya outta your mind! O'course not!", "No way!", "Not a chance!"] },
        {
            "how tall is violastro",
            [
                "Taller than y'all losers, that's for sure! Bwehehe!",
                "Y'all don't wanna know!",
                "Tall enough to steal y'alls Power Crystals!!",
            ]
        },
        { "is violastro attractive", ["You betcha!", "O'course!", "Sure am!"] },
        { "violastro tell me a joke", Jokes.List },
    };

    private static readonly Dictionary<string, string> QuestionsOneAnswer = new()
    {
        { "is violastro fat", ">:(" },
        { "how fat is violastro", ">:(" },
    };

    private readonly Random _random = new();

    public bool ShouldCancelOthers() => true;

    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        var hasBotMention = message.MentionedUsers.Any(u => u.Id == client.CurrentUser.Id);

        var normalizedMessage = hasBotMention ? StripLeadingMention(message.CleanContent) : message.CleanContent;
        normalizedMessage = normalizedMessage.ToLowerInvariant().Trim();
        normalizedMessage = RemovePunctuations(normalizedMessage);

        var answer = GetAnswer(normalizedMessage, hasBotMention);

        if (answer == null)
        {
            return false;
        }

        await message.Channel.SendMessageAsync(answer);

        return true;
    }

    private static string StripLeadingMention(string content)
    {
        var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return content;
        }

        return parts[0].StartsWith('@') ? string.Join(' ', parts.Skip(1)) : content;
    }

    private string? GetAnswer(string question, bool hasBotMention)
    {
        if (hasBotMention && question == "is this true")
        {
            return IsThisTrueAnswers[_random.Next(IsThisTrueAnswers.Count)];
        }

        foreach (var gameSynonym in GameSynonyms)
        {
            if (question.Contains(gameSynonym))
            {
                question = question.Replace(gameSynonym, "vv");
            }
        }

        if (QuestionsMultipleAnswers.TryGetValue(question, out var answers))
        {
            if (answers.Count == 1)
            {
                return answers[0];
            }

            return answers[_random.Next(answers.Count)];
        }

        if (QuestionsOneAnswer.TryGetValue(question, out var oneAnswer))
        {
            return oneAnswer;
        }

        return null;
    }

    private static string RemovePunctuations(string question)
    {
        return PunctuationRegex().Replace(question, "");
    }

    [GeneratedRegex("(~|`|!|@|#|$|%|^|&|\\*|\\(|\\)|{|}|\\[|\\]|;|:|\\\"|'|<|,|\\.|>|\\?|/|\\\\|\\||-|_|\\+|=)")]
    private static partial Regex PunctuationRegex();
}
