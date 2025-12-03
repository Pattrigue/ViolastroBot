namespace ViolastroBot.Extensions;

public static class StringExtensions
{
    public static string CapitalizeFirstCharacter(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToUpper(input[0]) + input[1..];
    }

    public static string CapitalizeFirstCharacterInEachWord(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var words = input.Split(' ');

        for (var i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                words[i] = words[i].CapitalizeFirstCharacter();
            }
        }

        return string.Join(" ", words);
    }
}
