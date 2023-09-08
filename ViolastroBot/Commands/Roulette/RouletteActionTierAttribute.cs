namespace ViolastroBot.Commands.Roulette;

public sealed class RouletteActionTierAttribute : Attribute
{
    public RouletteActionTier Tier { get; }

    public RouletteActionTierAttribute(RouletteActionTier tier)
    {
        Tier = tier;
    }
}