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

    public List<string> GetRandomWords(ushort min, ushort max)
    {
        int numWords = _random.Next(min, max + 1);
        List<string> words = new List<string>();

        for (int i = 0; i < numWords; i++)
        {
            WordType type = (WordType)_random.Next(0, Enum.GetNames(typeof(WordType)).Length);
            string[] wordArray = GetWordArray(type);
            int wordIndex = _random.Next(0, wordArray.Length);

            words.Add(wordArray[wordIndex]);
        }

        return words;
    }
    
    private string[] GetWordArray(WordType type)
    {
        return WordArrays[type];
    }
    
    private static string[] GetWordsFromFile(string fileName)
    {
        return File.ReadAllText($"{Directory}/{fileName}")
            .Split(Environment.NewLine)
            .Select(s => s.Trim())
            .ToArray();
    }
}