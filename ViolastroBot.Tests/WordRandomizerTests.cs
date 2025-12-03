using ViolastroBot.RandomWords;

namespace ViolastroBot.Tests;

public sealed class WordRandomizerTests
{
    private const int Seed = 42;

    [Theory]
    [InlineData(2, 4)]
    [InlineData(1, 5)]
    [InlineData(3, 3)]
    public void GetRandomWords_ReturnsTrimmedWords(ushort minWords, ushort maxWords)
    {
        WordRandomizer wordRandomizer = new(Seed);
        List<string> words = wordRandomizer.GetRandomWords(minWords, maxWords);
        
        foreach (var word in words)
        {
            Assert.DoesNotContain(' ', word);
        }
    }

    [Theory]
    [InlineData(2, 4)]
    [InlineData(1, 5)]
    [InlineData(3, 3)]
    public void GetRandomWords_ReturnsNonEmptyList(ushort minWords, ushort maxWords)
    {
        WordRandomizer wordRandomizer = new(Seed);
        List<string> words = wordRandomizer.GetRandomWords(minWords, maxWords);
        
        Assert.NotEmpty(words);
    }

    [Theory]
    [InlineData(2, 4)]
    [InlineData(1, 5)]
    [InlineData(3, 3)]
    public void GetRandomWords_ReturnsWithinSpecifiedRange(ushort minWords, ushort maxWords)
    {
        WordRandomizer wordRandomizer = new(Seed);
        List<string> words = wordRandomizer.GetRandomWords(minWords, maxWords);
        
        Assert.InRange(words.Count, minWords, maxWords);
    }
}