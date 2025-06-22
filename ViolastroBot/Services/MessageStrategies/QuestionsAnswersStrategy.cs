﻿using System.Text.RegularExpressions;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

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

    private static readonly List<string> MultiplayerAnswers = new()
    {
        "Yes! Bwehehe!",
        "I lied all these years! It will have multiplayer! Bwehehe!",
        "Y'all never saw it comin', but yes!"
    };

    private static readonly Dictionary<string, List<string>> QuestionsMultipleAnswers = new() 
    {
        {"when will vv be released", new List<string> { "Ya fool! Even I don't know that!", "Nobody knows!", "Sooner or later!" }},
        {"when will vv release", ReleaseAnswers},
        {"when is vv going to be released", ReleaseAnswers},
        {"when is vv going to release", ReleaseAnswers},
        {"when is vv gonna release", ReleaseAnswers},

        {"will vv have multiplayer", MultiplayerAnswers},
        {"will vv be multiplayer", MultiplayerAnswers},
        {"is vv gonna have multiplayer", MultiplayerAnswers},
        {"is vv going to have multiplayer", MultiplayerAnswers},
        {"is vv multiplayer", MultiplayerAnswers},

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
        {"violastro tell me a joke", Jokes.List }
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