using System.Text;
using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;
public sealed class ScoreboardService
{
    private const string ScoreboardSeparator = ": ";
    private const string ScoreboardFilePath = "scoreboard.txt"; // Define the path to the scoreboard file

    private sealed class Scoreboard
    {
        private readonly SocketGuild _guild;
        private readonly Dictionary<ulong, int> _scores;

        public Scoreboard(SocketGuild guild, Dictionary<ulong, int> scores)
        {
            _guild = guild;
            _scores = scores;
        }

        public bool TryGetUserScore(ulong userId, out int score) => _scores.TryGetValue(userId, out score);

        public void SetUserScore(ulong userId, int score) => _scores[userId] = score;

        public string Build(int? limit = null, bool prettify = false)
        {
            if (_scores.Count == 0)
            {
                return null;
            }
            
            StringBuilder newScoreboardContent = new StringBuilder();
    
            if (prettify)
            {
                SocketRole role = _guild.GetRole(Roles.NewRole);
                newScoreboardContent.AppendLine($"**🏆 `!roulette` SCOREBOARD 🏆**{Environment.NewLine}");
            }
            else
            {
                newScoreboardContent.AppendLine($"Scoreboard:{Environment.NewLine}");
            }
    
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
                    : $"{pair.Key}{ScoreboardSeparator}{pair.Value}";
    
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

        // Save the updated scoreboard to file
        await SaveScoreboardToFileAsync(scoreboard);
    }

    public async Task DisplayScoreboardAsync(SocketGuild guild, ISocketMessageChannel channel)
    {
        Scoreboard scoreboard = await GetScoreboard(guild);
        string scoreboardContent = scoreboard.Build(10, true);

        if (string.IsNullOrEmpty(scoreboardContent))
        {
            await channel.SendMessageAsync("Uh oh! The scoreboard is empty! Bwehehe!!");
            return;
        }
        
        await channel.SendMessageAsync(scoreboardContent, allowedMentions: AllowedMentions.None);
    }

    private static async Task<Scoreboard> GetScoreboard(SocketGuild guild)
    {
        Dictionary<ulong, int> scores = new Dictionary<ulong, int>();

        if (File.Exists(ScoreboardFilePath))
        {
            string[] lines = await File.ReadAllLinesAsync(ScoreboardFilePath);
            scores = ParseScoreboardFile(lines);
        }

        return new Scoreboard(guild, scores);
    }

    private static async Task SaveScoreboardToFileAsync(Scoreboard scoreboard)
    {
        string content = scoreboard.Build();
        
        await File.WriteAllTextAsync(ScoreboardFilePath, content);
    }

    private static Dictionary<ulong, int> ParseScoreboardFile(IEnumerable<string> lines)
    {
        Dictionary<ulong, int> scores = new Dictionary<ulong, int>();
        
        foreach (string line in lines)
        {
            string[] parts = line.Split(ScoreboardSeparator);

            if (parts.Length != 2) continue;

            string userIdStr = parts[0].Trim();
            string scoreStr = parts[1];

            bool isUserIdValid = ulong.TryParse(userIdStr, out ulong userId);
            bool isScoreValid = int.TryParse(scoreStr, out int score);

            if (isUserIdValid && isScoreValid)
            {
                scores[userId] = score;
            }
        }

        return scores;
    }
}