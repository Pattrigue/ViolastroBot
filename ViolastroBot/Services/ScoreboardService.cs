using System.Text;
using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;

public sealed class ScoreboardService : ServiceBase
{
    private sealed class Scoreboard(SocketGuild guild, Dictionary<ulong, int> scores)
    {
        public bool TryGetUserScore(ulong userId, out int score) => scores.TryGetValue(userId, out score);

        public void SetUserScore(ulong userId, int score) => scores[userId] = score;

        public string Build(int? limit = null, bool prettify = false)
        {
            if (scores.Count == 0)
            {
                return null;
            }

            var newScoreboardContent = new StringBuilder();

            if (prettify)
            {
                var role = guild.GetRole(Roles.NewRole);
                newScoreboardContent.AppendLine(
                    $"**🏆 `!roulette` {role.Mention} SCOREBOARD 🏆**{Environment.NewLine}"
                );
            }

            IEnumerable<KeyValuePair<ulong, int>> scoresToDisplay = scores;

            if (limit.HasValue)
            {
                scoresToDisplay = scores.OrderByDescending(pair => pair.Value).Take(limit.Value);
            }

            var rank = 1;

            foreach (var pair in scoresToDisplay)
            {
                var line = prettify
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
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var violastroBotFolderPath = Path.Combine(appDataPath, AppDomain.CurrentDomain.FriendlyName);

        Directory.CreateDirectory(violastroBotFolderPath);

        _scoreboardFilePath = Path.Combine(violastroBotFolderPath, ScoreboardFileName);
    }

    public async Task IncrementScoreboardAsync(SocketGuild guild, SocketUser user, int amount = 1)
    {
        var scoreboard = await GetScoreboard(guild);

        var userId = user.Id;

        if (scoreboard.TryGetUserScore(userId, out var score))
        {
            scoreboard.SetUserScore(userId, score + amount);
        }
        else
        {
            scoreboard.SetUserScore(userId, amount);
        }

        await SaveScoreboardToFileAsync(scoreboard);
    }

    public async Task DisplayScoreboardAsync(SocketGuild guild, ISocketMessageChannel channel)
    {
        var scoreboard = await GetScoreboard(guild);
        var scoreboardContent = scoreboard.Build(10, true);

        if (string.IsNullOrEmpty(scoreboardContent))
        {
            await channel.SendMessageAsync("Uh oh! The scoreboard is empty! Bwehehe!!");
            return;
        }

        await channel.SendMessageAsync(scoreboardContent, allowedMentions: AllowedMentions.None);
    }

    public async Task DisplayScoreAsync(SocketGuild guild, ISocketMessageChannel channel, SocketUser user)
    {
        var scoreboard = await GetScoreboard(guild);

        if (!scoreboard.TryGetUserScore(user.Id, out var score))
        {
            await channel.SendMessageAsync(
                $"*Synthetic LMAO*! <@{user.Id}> doesn't have any points! Bwehehe!!",
                allowedMentions: AllowedMentions.None
            );
            return;
        }

        await channel.SendMessageAsync(
            $"<@{user.Id}> has {score} points! Bwehehe!!",
            allowedMentions: AllowedMentions.None
        );
    }

    private async Task<Scoreboard> GetScoreboard(SocketGuild guild)
    {
        var scores = new Dictionary<ulong, int>();

        if (File.Exists(_scoreboardFilePath))
        {
            var lines = await File.ReadAllLinesAsync(_scoreboardFilePath);
            scores = ParseScoreboardFile(lines);
        }

        return new Scoreboard(guild, scores);
    }

    private async Task SaveScoreboardToFileAsync(Scoreboard scoreboard)
    {
        var content = scoreboard.Build();

        await File.WriteAllTextAsync(_scoreboardFilePath, content);
    }

    private static Dictionary<ulong, int> ParseScoreboardFile(IEnumerable<string> lines)
    {
        var scores = new Dictionary<ulong, int>();

        foreach (var line in lines)
        {
            var parts = line.Split(ScoreboardSeparator);

            if (parts.Length != 2)
                continue;

            var userIdStr = parts[0].Trim();
            var scoreStr = parts[1];

            var isUserIdValid = ulong.TryParse(userIdStr, out var userId);
            var isScoreValid = int.TryParse(scoreStr, out var score);

            if (isUserIdValid && isScoreValid)
            {
                scores[userId] = score;
            }
        }

        return scores;
    }
}
