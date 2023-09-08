namespace ViolastroBot.Commands.RouletteActions;

public sealed class RouletteActionTierAttribute : Attribute
{
    public RouletteActionTier Tier { get; }

    public RouletteActionTierAttribute(RouletteActionTier tier)
    {
        Tier = tier;
    }
}