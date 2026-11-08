using ViolastroBot.Features.Chat;

namespace ViolastroBot.Tests;

public sealed class ChatReplyWordFilterTests
{
    private static readonly string[] Stems = ["oople"];
    private static readonly char[] Prefixes = ['z', 'Q'];
    private static readonly string[] OtherWords = ["Blorp"];

    [Theory]
    [InlineData("You zoople!")]
    [InlineData("A Z-O-O-P-L-E right there")]
    [InlineData("q o o p l e")]
    [InlineData("oople then zoople")]
    [InlineData("big B.L.O.R.P energy")]
    public void ContainsBlockedWord_FindsWordsAcrossPunctuationAndCase(string content)
    {
        Assert.True(ChatReplyWordFilter.ContainsBlockedWord(content, Stems, Prefixes, OtherWords));
    }

    [Theory]
    [InlineData("Y'all ain't ready for this!")]
    [InlineData("oople at the start")]
    [InlineData("A poople is fine")]
    public void ContainsBlockedWord_AllowsOtherText(string content)
    {
        Assert.False(ChatReplyWordFilter.ContainsBlockedWord(content, Stems, Prefixes, OtherWords));
    }
}
