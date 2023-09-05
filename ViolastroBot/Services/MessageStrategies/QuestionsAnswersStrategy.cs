using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public sealed partial class QuestionsAnswersStrategy : IMessageStrategy
{
    private static readonly List<string> GameSynonyms = new() { "vv", "vibrant venture", "the game", "vibrantventure" };

    private static readonly List<string> ReleaseAnswers = new()
    {
        "Ya fool! Even I don't know that!",
        "Nobody knows!",
        "Sooner or later!"
    };

    private static readonly List<string> FaqAnswers = new()
    {
        "Bwehehe! Someone hasn't read the <#567008774288703507>!",
        "Y'all best read the <#567008774288703507>!",
        "Y'all should totally check out the <#567008774288703507>!"
    };

    private static readonly Dictionary<string, List<string>> QuestionsMultipleAnswers = new() 
    {
        {"when will vv be released", new List<string> { "Ya fool! Even I don't know that!", "Nobody knows!", "Sooner or later!" }},
        {"when will vv release", ReleaseAnswers},
        {"when is vv going to be released", ReleaseAnswers},
        {"when is vv going to release", ReleaseAnswers},
        {"when is vv gonna release", ReleaseAnswers},

        {"will vv have multiplayer", FaqAnswers},
        {"will vv be multiplayer", FaqAnswers},
        {"is vv gonna have multiplayer", FaqAnswers},
        {"is vv going to have multiplayer", FaqAnswers},
        {"is vv multiplayer", FaqAnswers},

        {"will vv be free", FaqAnswers},
        {"is vv gonna be free", FaqAnswers},
        {"is vv going to be free", FaqAnswers},
        {"is vv free", FaqAnswers},

        {"will vv cost money", FaqAnswers},
        {"is vv gonna cost money", FaqAnswers},
        {"is vv going to cost money", FaqAnswers},
        {"does vv cost money", FaqAnswers},

        {"is violastro bald", new List<string> { "What! Are ya outta your mind! O'course not!", "No way!", "Not a chance!" }},
        {"how tall is violastro", new List<string> { "Taller than y'all losers, that's for sure! Bwehehe!", "Y'all don't wanna know!", "Tall enough to steal y'alls Power Crystals!!" }},
        {"is violastro attractive", new List<string> { "You betcha!", "O'course!", "Sure am!" }},
        {"violastro tell me a joke", new List<string> 
        { 
            "Jur Loogman!",
            "I'm reading a book about anti-gravity. It's impossible to put down!",
            "Whaddya call someone with no body and no nose? Nobody knows! Bwehehe!",
            "Don't trust atoms. They make up everything!",
            "I used to have a job at a calendar factory, but I got the sack because I took a couple of days off!!",
            "Y'all know what the loudest pet you can get is? A trumpet!!",
            "Why did the scarecrow win an award? Because he was outstanding in his field!",
            "Why couldn't the bicycle stand up by itself? It was two tired!",
            "Did you hear about the restaurant on the moon? Great food, no atmosphere!",
            "I'm finna run out of jokes soon!",
            "Did you hear the rumor about butter? Well, I'm not going to spread it!",
            "Why do you never see elephants hiding in trees? Because they're so good at it!",
            "How does a penguin build its house? Igloos it together!",
            "Why don't skeletons ever go trick or treating? Because they have no body to go with!",
            "Why did the old man fall in the well? Because he couldn't see that well!",
            "Why did the invisible man turn down the job offer? He couldn't see himself doing it!",
            "I'm so good at sleeping I can do it with my eyes closed!",
            "I thought about going on an all-almond diet… But that's just nuts!",
            "I would avoid the sushi if I was you. It's a little fishy!",
            "<https://www.roblox.com/games/433264537/DISCONTINUED-READ-DESC-Vibrant-Venture-v1-4-1>",
            "Two goldfish are in a tank. One says to the other, 'Do you know how to drive this thing?'",
            "Did you hear about the Italian chef who died? He pasta way!",
            "No! I'm busy conquering the world!",
            "What's orange and sounds like a parrot? A carrot!",
            "2020!",
            "What's a ninja's favorite type of shoes? Sneakers!",
            "https://smurfs.fandom.com/wiki/Death"
        }}
    };

    private static readonly Dictionary<string, string> QuestionsOneAnswer = new()
    {
        { "is violastro fat", ">:(" },
        { "how fat is violastro", ">:(" }
    };
    
    private readonly Random _random = new();

    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        string answer = GetAnswer(message.Content);

        if (answer == null)
        {
            return false;
        }
        
        await message.Channel.SendMessageAsync(answer);
        
        return true;
    }
    
    public bool ShouldCancelOthers() => true;

    private string GetAnswer(string question)
    {
        string formattedQuestion = RemovePunctuations(question.ToLower());

        foreach (string gameSynonym in GameSynonyms)
        {
            if (formattedQuestion.Contains(gameSynonym))
            {
                formattedQuestion = formattedQuestion.Replace(gameSynonym, "vv");
            }
        }

        if (QuestionsMultipleAnswers.TryGetValue(formattedQuestion, out List<string> answers))
        {
            if (answers.Count == 1)
            {
                return answers[0];
            }

            return answers[_random.Next(answers.Count)];
        }

        if (QuestionsOneAnswer.TryGetValue(formattedQuestion, out string oneAnswer))
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