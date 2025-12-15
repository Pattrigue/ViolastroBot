using JetBrains.Annotations;

namespace ViolastroBot.Features.Roulette;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
[AttributeUsage(AttributeTargets.Class)]
public sealed class RouletteActionTierAttribute(RouletteActionTier tier) : Attribute
{
    public RouletteActionTier Tier { get; } = tier;
}
