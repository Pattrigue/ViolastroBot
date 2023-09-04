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
}