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

    [Theory]
    [InlineData("Bwuh, ya know that?! Synthetic bwehehe!", "Bwuh, ya know that?! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! Synthethic bwehehe", "Ya fool! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! *Synthetic bwehehe!*", "Ya fool! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! Clank!", "Ya fool! *clank*")]
    [InlineData("Bzzt! Ya fool!", "*bzzt* Ya fool!")]
    [InlineData("Ya fool! *whirr*", "Ya fool! *whirr*")]
    [InlineData("Go ask someone else! whirr (stage direction)", "Go ask someone else! *whirr*")]
    [InlineData("Go ask someone else! Bzzt.", "Go ask someone else! *bzzt*")]
    [InlineData("That ain't worth a clank", "That ain't worth a clank")]
    public void Sanitize_WrapsRobotNoisesInAsterisks(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Ya fool! *bzzt", "Ya fool! *bzzt*")]
    [InlineData("Ya fool! *clank", "Ya fool! *clank*")]
    [InlineData("Ya fool! *whirr!", "Ya fool! *whirr*")]
    [InlineData("*bzzt Mind the blades!", "*bzzt* Mind the blades!")]
    [InlineData("Ya fool! *Synthetic bwehehe!", "Ya fool! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! *Synthetic bwehehe", "Ya fool! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! *bzzt*", "Ya fool! *bzzt*")]
    [InlineData("Ya fool! *bzzt!*", "Ya fool! *bzzt*")]
    [InlineData("Ya fool! *Synthetic bwehehe*", "Ya fool! *Synthetic bwehehe!*")]
    [InlineData("Ya fool! *clanker", "Ya fool! *clanker")]
    public void Sanitize_NormalizesStarredRobotNoises(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Ya fool! Cackle!", "Ya fool!")]
    [InlineData("Ya fool! (Grunt)", "Ya fool!")]
    [InlineData("Grunt! Ya fool!", "Ya fool!")]
    [InlineData("I heard a grunt in the forest!", "I heard a grunt in the forest!")]
    public void Sanitize_RemovesSpokenStageNoises(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Ya fool! *Stomping foot*", "Ya fool!")]
    [InlineData("Ya fool! (Popcorn crunch)", "Ya fool!")]
    [InlineData("Ya fool! [snicker]", "Ya fool!")]
    [InlineData("*Twirls dramatically* Ya fool!", "Ya fool!")]
    [InlineData("Ya fool! [Bwahaha!] *Applause*", "Ya fool!")]
    [InlineData("Ya fool! [whirr]", "Ya fool!")]
    [InlineData("Ya fool! <noise>", "Ya fool!")]
    public void Sanitize_RemovesNarratedActions(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("That is *really* rude, ya fool!")]
    [InlineData("Ya fool! *bzzt* Mind the blades!")]
    public void Sanitize_KeepsEmphasisAndAllowedNoises(string reply)
    {
        Assert.Equal(reply, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("Bwehehe! Private note: Try this angle", "Bwehehe!")]
    [InlineData("Bwehehe! [private note: Try this angle", "Bwehehe!")]
    public void Sanitize_RemovesLeakedPrivateNotes(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
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
    [InlineData("Bwehehe!\n\n*(Stage direction: Applause)*", "Bwehehe!")]
    [InlineData("Bwehehe! Wha--?! (stage direction)", "Bwehehe! Wha--?!")]
    [InlineData("Bwehehe! [Stage direction]", "Bwehehe!")]
    public void Sanitize_RemovesStageDirections(string reply, string expected)
    {
        Assert.Equal(expected, ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("X! Y! Z! T! A! Bwuh?")]
    [InlineData("Here ya go: X-Y-Z!")]
    [InlineData("x. y. z. Bwehehe!")]
    [InlineData("X Y Z")]
    public void Sanitize_RejectsSpelledOutLetters(string reply)
    {
        Assert.Null(ChatReplySanitizer.Sanitize(reply, false));
    }

    [Theory]
    [InlineData("I ain't a fool, y'all!")]
    [InlineData("Plan B, ya fool!")]
    [InlineData("I'm a robot and I'd win!")]
    public void Sanitize_AllowsOrdinarySingleLetters(string reply)
    {
        Assert.Equal(reply, ChatReplySanitizer.Sanitize(reply, false));
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
