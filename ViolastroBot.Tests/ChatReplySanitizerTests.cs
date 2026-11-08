using ViolastroBot.Features.Chat;

namespace ViolastroBot.Tests;

public sealed class ChatReplySanitizerTests
{
    [Theory]
    [InlineData("[IGNORE]", true)]
    [InlineData("[IGNORE] Bwehehe!", true)]
    [InlineData("Y'all can't ignore me!", false)]
    public void IsRefusal_DetectsOnlyTheMarker(string reply, bool expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.IsRefusal(reply));
    }

    [Fact]
    public void Sanitize_NormalizesCurlyQuotes()
    {
        Assert.Equal("Y'all ain't ready!", ChatReplySanitizer.Sanitize("Y’all ain’t ready!", false));
    }

    [Theory]
    [InlineData("Bwehehe! \U0001F3AD\U0001F4A5", "Bwehehe!")]
    [InlineData("Howdy ❤️ y'all!", "Howdy y'all!")]
    public void Sanitize_RemovesEmoji(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Hey @somebody!", "Hey somebody!")]
    [InlineData("Hey <@123456789> and <@!42> and <@&7>!", "Hey and and !")]
    [InlineData("Go to <#567008774288703507> now!", "Go to now!")]
    public void Sanitize_RemovesMentions(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Bwehehe! (Stage direction: Use no noises in this reply.)", "Bwehehe!")]
    [InlineData("Bwehehe!\n\n(Stage direction: Use no noises in this", "Bwehehe!")]
    public void Sanitize_RemovesStageDirections(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("@everyone FREE NITRO CLICK HERE")]
    [InlineData("Hey @HERE, listen up!")]
    public void Sanitize_RejectsMassPings(string reply)
    {
        Assert.Null(ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Free nitro at https://example.com ya fool!")]
    [InlineData("Go to WWW.example.com!")]
    [InlineData("Join discord.gg/abc123!")]
    public void Sanitize_RejectsLinks(string reply)
    {
        Assert.Null(ChatReplySanitizer.Sanitize(reply, false));
    }

    [Fact]
    public void Sanitize_RejectsEchoedInstructions()
    {
        const string reply = "Fine! You sound like a Southern American showman: loud, flamboyant, smug!";

        Assert.Null(ChatReplySanitizer.Sanitize(reply, false));
    }

    [Fact]
    public void Sanitize_AllowsSharedShortPhrases()
    {
        const string reply = "I'm too busy conquerin' the world, ya fool!";

        Assert.Equal(reply, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Fact]
    public void Sanitize_AllowsRepeatingPublicLore()
    {
        const string reply = "During a storm he stole the Power Crystals from the Temple and escaped through a portal!";

        Assert.Equal(reply, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Bwehehe! Y'all ain't ready for what I'm fixin", "Bwehehe!")]
    [InlineData("Howdy! *Synthetic bwehehe!* And another th", "Howdy! *Synthetic bwehehe!*")]
    public void Sanitize_TrimsTruncatedRepliesToLastSentence(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, true));
    }

    [Theory]
    [InlineData("no punctuation at all", true)]
    [InlineData("   ", false)]
    [InlineData("\U0001F3AD", false)]
    public void Sanitize_RejectsRepliesWithNothingLeft(string reply, bool wasTruncated)
    {
        Assert.Null(ChatReplySanitizer.Sanitize(reply, wasTruncated));
    }

    [Fact]
    public void Sanitize_RejectsRepliesOverDiscordLimit()
    {
        Assert.Null(ChatReplySanitizer.Sanitize(new string('a', 2001), false));
    }
}
