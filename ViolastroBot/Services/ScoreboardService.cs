using System.Text;
using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;

public sealed class ScoreboardService : ServiceBase
{
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
                newScoreboardContent.AppendLine($"**🏆 `!roulette` {role.Mention} SCOREBOARD 🏆**{Environment.NewLine}");
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
    
    private const string ScoreboardSeparator = ": ";
    private const string ScoreboardFileName = "scoreboard.txt";

    private readonly string _scoreboardFilePath;

    public ScoreboardService()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string violastroBotFolderPath = Path.Combine(appDataPath, AppDomain.CurrentDomain.FriendlyName);
                
        Directory.CreateDirectory(violastroBotFolderPath);
       
        _scoreboardFilePath = Path.Combine(violastroBotFolderPath, ScoreboardFileName);
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

    public async Task DisplayScoreAsync(SocketGuild guild, ISocketMessageChannel channel, SocketUser user)
    {
        Scoreboard scoreboard = await GetScoreboard(guild);

        if (!scoreboard.TryGetUserScore(user.Id, out int score))
        {
            await channel.SendMessageAsync($"*Synthetic LMAO*! <@{user.Id}> doesn't have any points! Bwehehe!!", allowedMentions: AllowedMentions.None);
            return;
        }

        await channel.SendMessageAsync($"<@{user.Id}> has {score} points! Bwehehe!!", allowedMentions: AllowedMentions.None);
    }
    
    private async Task<Scoreboard> GetScoreboard(SocketGuild guild)
    {
        Dictionary<ulong, int> scores = new Dictionary<ulong, int>();

        if (File.Exists(_scoreboardFilePath))
        {
            string[] lines = await File.ReadAllLinesAsync(_scoreboardFilePath);
            scores = ParseScoreboardFile(lines);
        }

        return new Scoreboard(guild, scores);
    }

    private async Task SaveScoreboardToFileAsync(Scoreboard scoreboard)
    {
        string content = scoreboard.Build();
        
        await File.WriteAllTextAsync(_scoreboardFilePath, content);
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