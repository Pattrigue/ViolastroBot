namespace ViolastroBot.RandomWords;

public sealed class WordRandomizer
{
    private const string Directory = "RandomWords";

    private static readonly Dictionary<WordType, string[]> WordArrays = new Dictionary<WordType, string[]>();

    private readonly Random _random;

    static WordRandomizer()
    {
        WordArrays[WordType.Noun] = GetWordsFromFile("nouns.txt");
        WordArrays[WordType.Verb] = GetWordsFromFile("verbs.txt");
        WordArrays[WordType.Adjective] = GetWordsFromFile("adjectives.txt");
    }

    public WordRandomizer(int seed = -1)
    {
        _random = seed == -1 ? new Random() : new Random(seed);
    }

    public WordRandomizer(ulong seed)
    {
        _random = new Random((int)(seed >> 32) ^ (int)seed);
    }

    public string GetRandomWord()
    {
        var type = (WordType)_random.Next(0, Enum.GetNames(typeof(WordType)).Length);
        var wordArray = GetWordArray(type);
        var wordIndex = _random.Next(0, wordArray.Length);

        return wordArray[wordIndex];
    }

    public List<string> GetRandomWords(ushort min, ushort max)
    {
        var numWords = _random.Next(min, max + 1);
        var words = new List<string>();

        for (var i = 0; i < numWords; i++)
        {
            words.Add(GetRandomWord());
        }

        return words;
    }

    private string[] GetWordArray(WordType type)
    {
        return WordArrays[type];
    }

    private static string[] GetWordsFromFile(string fileName)
    {
        return File.ReadAllText($"{Directory}/{fileName}").Split(Environment.NewLine).Select(s => s.Trim()).ToArray();
    }
}
