namespace ViolastroBot.Features.Roulette;

public sealed class RouletteActionTierAttribute(RouletteActionTier tier) : Attribute
{
    public RouletteActionTier Tier { get; } = tier;
}
