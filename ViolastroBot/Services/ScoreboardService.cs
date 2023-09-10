using System.Text;
using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;

public sealed class ScoreboardService : ServiceBase
{
    private const string ScoreboardSeparator = ": ";
    
    private sealed class Scoreboard
    {
        public SocketTextChannel Channel { get; }
        public IUserMessage Message { get; }
        
        private readonly Dictionary<ulong, int> _scores;

        public Scoreboard(SocketTextChannel channel, IUserMessage message, Dictionary<ulong, int> scores)
        {
            Channel = channel;
            Message = message;
            _scores = scores;
        }
        
        public bool TryGetUserScore(ulong userId, out int score) => _scores.TryGetValue(userId, out score);

        public void SetUserScore(ulong userId, int score) => _scores[userId] = score;

        public string Build(int? limit = null, bool prettify = false)
        {
            StringBuilder newScoreboardContent = new StringBuilder();

            if (prettify)
            {
                SocketRole role = Channel.Guild.GetRole(Roles.NewRole);
                newScoreboardContent.AppendLine($"**🏆 `!roulette` {role.Mention} SCOREBOARD 🏆**{Environment.NewLine}");
            }
            else
            {
                newScoreboardContent.AppendLine($"Scoreboard:{Environment.NewLine}");
            }

            Console.WriteLine($"Building scoreboard with limit: {limit} for a total of {_scores.Count} scores.");
            IEnumerable<KeyValuePair<ulong, int>> scoresToDisplay = _scores;

            if (limit.HasValue)
            {
                scoresToDisplay = _scores.OrderByDescending(pair => pair.Value).Take(limit.Value);
            }

            int rank = 1;
            
            foreach (KeyValuePair<ulong, int> pair in scoresToDisplay)
            {
                string line = prettify
                    ? $"**{rank}) <@{pair.Key}>{ScoreboardSeparator}{pair.Value}**"
                    : $"<@{pair.Key}>{ScoreboardSeparator}{pair.Value}";

                newScoreboardContent.AppendLine(line);
                rank++;
            }

            return newScoreboardContent.ToString();
        }
    }
    
    public async Task IncrementScoreboardAsync(SocketGuild guild, SocketUser user)
    {
        Scoreboard scoreboard = await GetScoreboard(guild);
        
        ulong userId = user.Id;
        
        if (scoreboard.TryGetUserScore(userId, out int score))
        {
            scoreboard.SetUserScore(userId, score + 1);
        }
        else
        {
            scoreboard.SetUserScore(userId, 1);
        }

        // Build new scoreboard message content
        string scoreboardContent = scoreboard.Build();

        // Update the scoreboard message
        if (scoreboard.Message != null)
        {
            await scoreboard.Message.ModifyAsync(msg => msg.Content = scoreboardContent);
        }
        else
        {
            await scoreboard.Channel.SendMessageAsync(scoreboardContent, allowedMentions: AllowedMentions.None);
        }
    }

    public async Task DisplayScoreboardAsync(SocketGuild guild, ISocketMessageChannel channel)
    {
        Scoreboard scoreboard = await GetScoreboard(guild);

        await channel.SendMessageAsync(scoreboard.Build(10, true), allowedMentions: AllowedMentions.None);
    }

    private static async Task<Scoreboard> GetScoreboard(SocketGuild guild)
    {
        SocketTextChannel channel = guild.GetTextChannel(Channels.Scoreboard);
        
        IEnumerable<IMessage> messages = await channel.GetMessagesAsync(1).FlattenAsync();
        IUserMessage scoreboardMessage = messages.FirstOrDefault() as IUserMessage;

        // If scoreboard message exists, parse it - otherwise, initialize new scoreboard
        Dictionary<ulong, int> scoreboard = scoreboardMessage != null 
            ? ParseScoreboardMessage(scoreboardMessage.Content) 
            : new Dictionary<ulong, int>();

        return new Scoreboard(channel, scoreboardMessage, scoreboard);
    }

    private static Dictionary<ulong, int> ParseScoreboardMessage(string content)
    {
        Dictionary<ulong, int> scores = new Dictionary<ulong, int>();
        string[] lines = content.Split(Environment.NewLine);

        foreach (string line in lines.Skip(1)) // Skip the "Scoreboard:" line
        {
            string[] parts = line.Split($"{ScoreboardSeparator}");

            if (parts.Length != 2) continue;

            string userIdMention = parts[0];
            string scoreStr = parts[1];

            bool isUserIdValid = MentionUtils.TryParseUser(userIdMention, out ulong userId);
            bool isScoreValid = int.TryParse(scoreStr, out int score);

            if (isUserIdValid && isScoreValid)
            {
                scores[userId] = score;
            }
        }

        return scores;
    }
}